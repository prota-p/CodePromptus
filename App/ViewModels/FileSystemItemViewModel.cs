using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using DynamicData;
using CodePromptus.App.Models;
using System.Diagnostics;
using UtfUnknown.Core.Models.SingleByte.Russian;

namespace CodePromptus.App.ViewModels;

public class FileSystemItemViewModel(FileSystemItem model) : ViewModelBase
{
    private FileSystemItem _model = model;

    public FileSystemItem Model => _model;
    public string Name => _model.Name;
    public string FullPath => _model.FullPath;
    public string RelativePath => _model.RelativePath;
    public bool IsDirectory => _model.IsDirectory;
    public string Icon => IsDirectory ? "📁" : "📄";

    public bool IsSelected
    {
        get => _model.IsSelected;
        set
        {
            if (_model.IsSelected != value)
            {
                _model.IsSelected = value;
                this.RaisePropertyChanged();
            }
        }
    }

    [Reactive]
    public bool IsExpanded { get; set; }

    public ObservableCollection<FileSystemItemViewModel> Children { get; private set; } = [];

    public void SetChildren(IEnumerable<FileSystemItemViewModel> children)
    {
        Children.Clear();
        Children.AddRange(children);
    }

    public void SetModel(FileSystemItem newModel)
    {
        _model = newModel;
    }

    public IEnumerable<FileSystemItemViewModel> GetAllDescendants()
    {
        yield return this;

        foreach (var child in Children)
        {
            foreach (var fileDescendant in child.GetAllDescendants())
            {
                yield return fileDescendant;
            }
        }
    }
}
