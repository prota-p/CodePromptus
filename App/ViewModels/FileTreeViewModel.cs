using Avalonia.Controls;
using CodePromptus.App.Helpers;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Models;
using CodePromptus.App.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace CodePromptus.App.ViewModels;

public class FileTreeViewModel : ViewModelBase, IDisposable
{
    private readonly FileSystemItemViewModelFactory _fileSystemItemViewModelFactory;
    private readonly FileTreeRepositoryService _fileTreeRepositoryService;
    private readonly FolderHistoryRepositoryService _folderHistoryRepositoryService;
    private readonly ILogService<FileTreeViewModel> _logger;
    private readonly ToastService _toastService;
    private readonly Subject<(FileSystemItemViewModel Item, bool IsSelected)> _fileOrFolderSelectionChangedSubject = new();
    private readonly Subject<Unit> _fileTreeStructureReloadedSubject = new();

    private CompositeDisposable _selectionChangeDisposables = [];

    [Reactive]
    public ObservableCollection<FileSystemItemViewModel> RootItemVms { get; private set; } = [];
    [Reactive]
    public string GitignorePath { get; set; } = string.Empty;
    [ObservableAsProperty]
    public string GitignoreFileName { get; }
    [Reactive]
    public string FilterText { get; set; } = string.Empty;
    [Reactive]
    public bool IsLoading { get; private set; }
    [Reactive]
    public ObservableCollection<FolderHistoryEntry> HistoryEntries { get; private set; } = [];
    [Reactive]
    public FolderHistoryEntry? SelectedHistoryEntry { get; set; }
    public ReactiveCommand<Unit, Unit> ReloadCommand { get; } = ReactiveCommand.Create(() => { });
    public IObservable<(FileSystemItemViewModel Item, bool IsSelected)> FileOrFolderSelectionChanged => _fileOrFolderSelectionChangedSubject.AsObservable();
    public IObservable<Unit> FileTreeStructureReloaded => _fileTreeStructureReloadedSubject.AsObservable();

#pragma warning disable CS8618
    public FileTreeViewModel() { DesignModeGuard.ThrowIfNotDesignMode(); } // For design-time data
    public FileTreeViewModel(
           FileSystemItemViewModelFactory fileSystemItemViewModelFactory,
           FileTreeRepositoryService fileTreeRepositoryService,
           FolderHistoryRepositoryService folderHistoryRepositoryService,
           ToastService toastService,
           ILogService<FileTreeViewModel> logger)
    {
        _fileSystemItemViewModelFactory = fileSystemItemViewModelFactory;
        _fileTreeRepositoryService = fileTreeRepositoryService;
        _folderHistoryRepositoryService = folderHistoryRepositoryService;
        _toastService = toastService;
        _logger = logger;

        ReloadCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (RootItemVms.Count > 0)
            {
                await LoadFileSystemAsync(RootItemVms[0].FullPath, GitignorePath);
            }
        });

        this.WhenAnyValue(x => x.FilterText)
            .Skip(1)
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Subscribe(_ =>
            {
                if (RootItemVms.Count > 0)
                {
                    ConstructRootItemVmFromModel(RootItemVms[0].Model);
                }
            });

        this.WhenAnyValue(x => x.SelectedHistoryEntry)
            .Skip(1)
            .Where(x => x != null)
            .Select(x => new { Entry = x, Key = new { x!.FolderPath, x.GitignorePath } })
            .DistinctUntilChanged(x => x.Key)
            .Select(x => x.Entry!)
            .SelectMany(entry =>                             
                Observable.FromAsync(() =>
                    LoadFileSystemAsync(entry.FolderPath, entry.GitignorePath)))
            .Subscribe();

        this.WhenAnyValue(x => x.GitignorePath)
            .Select(path => Path.GetFileName(path) ?? string.Empty)
            .ToPropertyEx(this, x => x.GitignoreFileName);
    }

    public async Task InitialLoadFileSystemAsync()
    {
        var folderHistory = await _folderHistoryRepositoryService.LoadFolderHistoryAsync();
        SelectedHistoryEntry = folderHistory.GetLatestEntry();
    }

    public async Task LoadFileSystemAsync(string newRootFolderPath, string newOriginalInputGitignorePath)
    {
        try
        {
            IsLoading = true;

            var folderHistory = await _folderHistoryRepositoryService.LoadFolderHistoryAsync();
            
            UpdateCurrentHistoryEntrySelection(folderHistory, GetCurrentRoot());
            var (newModelRoot, newGitignorePath) = await _fileTreeRepositoryService.LoadFileTreeAsync(newRootFolderPath, newOriginalInputGitignorePath);
            RemoveNonExistentPathsFromHistoryEntrySelection(folderHistory, newModelRoot, newGitignorePath);
            LoadLatestHistoryEntries(newModelRoot, folderHistory);
            UpdateSelectedStateByHistoryEntry(newModelRoot, folderHistory);
            ConstructRootItemVmFromModel(newModelRoot);
            GitignorePath = newGitignorePath;
            SubscribeToSelectionChanges();
            _fileTreeStructureReloadedSubject.OnNext(Unit.Default);
            
            await _folderHistoryRepositoryService.SaveFolderHistoryAsync(folderHistory);
        }
        catch (Exception ex)
        {
            HandleLoadException(ex, newRootFolderPath);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SaveCurrentHistoryEntrySelectionAsync()
    {
        var folderHistory = await _folderHistoryRepositoryService.LoadFolderHistoryAsync();
        UpdateCurrentHistoryEntrySelection(folderHistory, GetCurrentRoot());
        await _folderHistoryRepositoryService.SaveFolderHistoryAsync(folderHistory);
    }

    private FileSystemItem? GetCurrentRoot()
    {
        return RootItemVms.Count > 0 ? RootItemVms[0].Model : null;
    }

    private static void UpdateCurrentHistoryEntrySelection(FolderHistory folderHistory, FileSystemItem? currentRoot)
    {
        if (currentRoot != null)
        {
            var existingEntry = folderHistory.GetEntry(currentRoot);
            existingEntry?.UpdateSelectedState(currentRoot);
        }
    }

    private void LoadLatestHistoryEntries(FileSystemItem newModelRoot, FolderHistory folderHistory)
    {
        HistoryEntries = new ObservableCollection<FolderHistoryEntry>(folderHistory.Entries);
        var currentEntry = folderHistory.GetEntry(newModelRoot);
        SelectedHistoryEntry = currentEntry;
    }

    private static void UpdateSelectedStateByHistoryEntry(FileSystemItem newModelRoot, FolderHistory folderHistory)
    {
        newModelRoot.UpdateSelectedStateToDescendants(
            folderHistory.GetEntry(newModelRoot)?.SelectedFilePathList ?? []
        );
    }

    private static void RemoveNonExistentPathsFromHistoryEntrySelection(FolderHistory folderHistory, FileSystemItem newModelRoot, string newGitignorePath)
    {
        var newEntry = folderHistory.GetOrCreateEntry(newModelRoot, newGitignorePath);
        newEntry.RemoveNonExistentPathsFromSelection(newModelRoot);
    }

    private void HandleLoadException(Exception ex, string? newRootFolderPath)
    {
        _logger.Error(ex, "Error loading file system. Path: {Path}, Error: {Error}",
            newRootFolderPath ?? "[null]", ex.Message);
        var pathInfo = string.IsNullOrEmpty(newRootFolderPath) ? "no path selected" : $"path: {newRootFolderPath}";
        _toastService.ShowError($"Failed to load folder ({pathInfo}). {ex.Message}");
    }

    private void SubscribeToSelectionChanges()
    {
        _selectionChangeDisposables.Dispose();
        _selectionChangeDisposables = [];

        var allFileItems = RootItemVms.SelectMany(rootItem => rootItem.GetAllDescendants());

        foreach (var item in allFileItems)
        {
            item.WhenAnyValue(x => x.IsSelected)
                .Skip(1) // skip the initial value
                .Subscribe(isSelected => _fileOrFolderSelectionChangedSubject.OnNext((item, isSelected)))
                .DisposeWith(_selectionChangeDisposables);
        }
    }

    private void ConstructRootItemVmFromModel(FileSystemItem rootItem)
    {
        var rootVm = _fileSystemItemViewModelFactory.CreateFromModel(rootItem, FilterText);
        RootItemVms = [rootVm];
    }

    public void Dispose()
    {
        _selectionChangeDisposables.Dispose();
        GC.SuppressFinalize(this);
    }
}
