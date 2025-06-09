using Avalonia.Controls;
using Avalonia.Interactivity;
using CodePromptus.App.Helpers;
using CodePromptus.App.ViewModels;
using System;
using System.IO;

namespace CodePromptus.App.Views;

public partial class FileTreeView : UserControl
{
    public FileTreeView()
    {
        InitializeComponent();
        if (Design.IsDesignMode) return; // Skip initialization in design mode
        DataContextChanged += OnDataContextChanged;
    }

    private async void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is FileTreeViewModel vm)
        {
            await vm.InitialLoadFileSystemAsync();
        }
    }

    private async void OnOpenFileClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FileTreeViewModel vm && VisualRoot is Window window)
        {
            var rootFolder = await DialogHelper.ShowOpenFolderAsync(window, "Select code folder");
            rootFolder = rootFolder?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!string.IsNullOrEmpty(rootFolder))
            {
                await vm.LoadFileSystemAsync(rootFolder, string.Empty);
            }
        }
    }

    private async void OnSelectGitignoreClicked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FileTreeViewModel vm && VisualRoot is Window window)
        {
            var rootVm = vm.RootItemVms.Count > 0 ? vm.RootItemVms[0] : null;
            if (rootVm == null) return;
            var gitignoreFilePath = await DialogHelper.ShowOpenFileAsync(
                window,
                "Select gitignore file",
                ["gitignore"]);

            if (!string.IsNullOrEmpty(gitignoreFilePath))
            {
                await vm.LoadFileSystemAsync(rootVm.FullPath, gitignoreFilePath);
            }
        }
    }
}
