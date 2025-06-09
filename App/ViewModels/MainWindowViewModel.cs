using CodePromptus.App.Helpers;
using CodePromptus.App.Infrastructure;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodePromptus.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ILogService<MainWindowViewModel> _logger;
    private readonly CompositeDisposable _disposables = [];

    private readonly Subject<Unit> _regeneratePromptSubject = new();

    [Reactive]
    public FileTreeViewModel FileTreeVm { get; private set; }
    [Reactive]
    public SelectedFilesViewModel SelectedFilesVm { get; private set; }
    [Reactive]
    public PromptDisplayViewModel PromptDisplayVm { get; private set; }
    [Reactive]
    public ToastControllerViewModel ToastControllerVm { get; private set; }

#pragma warning disable CS8618
    public MainWindowViewModel() { DesignModeGuard.ThrowIfNotDesignMode(); } //For design-time data
#pragma warning restore CS8618

    public MainWindowViewModel(
        ILogService<MainWindowViewModel> logger,
        FileTreeViewModel fileTreeViewModel,
        SelectedFilesViewModel selectedFilesViewModel,
        PromptDisplayViewModel promptDisplayViewModel,
        ToastControllerViewModel toastControllerViewModel)
    {
        _logger = logger;
        FileTreeVm = fileTreeViewModel;
        SelectedFilesVm = selectedFilesViewModel;
        PromptDisplayVm = promptDisplayViewModel;
        ToastControllerVm = toastControllerViewModel;

        FileTreeVm.FileOrFolderSelectionChanged
            .Subscribe(args =>
            {
                if (args.Item.IsDirectory)
                {
                    foreach(var child in args.Item.Children)
                    {
                        child.IsSelected = args.IsSelected;
                    }
                }
                else
                {
                    if (args.IsSelected) SelectedFilesVm.SelectedItems.Add(args.Item);
                    else SelectedFilesVm.SelectedItems.Remove(args.Item);
                }
                _regeneratePromptSubject.OnNext(Unit.Default);

            })
            .DisposeWith(_disposables);

        FileTreeVm.FileTreeStructureReloaded
            .Subscribe(_ =>
            {
                SelectedFilesVm.SelectedItems.Clear();
                SelectedFilesVm.SelectedItems.AddRange(FileTreeVm.RootItemVms[0].GetAllDescendants().Where(item => !item.IsDirectory && item.IsSelected));
                _regeneratePromptSubject.OnNext(Unit.Default);
            })
            .DisposeWith(_disposables);

        _regeneratePromptSubject
            .Throttle(TimeSpan.FromMilliseconds(300))      
            .Select(_ =>                                   
                Observable.FromAsync(async () =>           
                {
                    Debug.WriteLine("Regenerating prompt...");
                    await promptDisplayViewModel.GeneratePromptAsync(
                             FileTreeVm.RootItemVms.Select(item => item.Model));
                    await fileTreeViewModel.SaveCurrentHistoryEntrySelectionAsync();
                }))
            .Switch()
            .Subscribe();
    }

    public void Dispose()
    {
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}