using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace CodePromptus.App.ViewModels;

public class SelectedFilesViewModel : ViewModelBase
{
    [Reactive]
    public ObservableCollection<FileSystemItemViewModel> SelectedItems { get; set; } = [];

    [ObservableAsProperty]
    public bool HasSelectedItems { get; }

    public SelectedFilesViewModel()
    {
        this.WhenAnyValue(x => x.SelectedItems.Count)
            .Select(count => count > 0)
            .ToPropertyEx(this, x => x.HasSelectedItems);
    }
}