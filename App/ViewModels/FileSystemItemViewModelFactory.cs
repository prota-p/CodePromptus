using CodePromptus.App.Configuration;
using CodePromptus.App.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;

namespace CodePromptus.App.ViewModels;

public class FileSystemItemViewModelFactory
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = Constants.MaxFileSystemItemViewModelCacheSize });
    private static readonly MemoryCacheEntryOptions _cacheOptions = new MemoryCacheEntryOptions().SetSize(1);


    public FileSystemItemViewModel CreateFromModel(FileSystemItem model, string filterPattern = "")
    {
        _cache.TryGetValue(model.FullPath, out FileSystemItemViewModel? viewModel);
        if (viewModel == null) { 
            viewModel = new FileSystemItemViewModel(model);
            _cache.Set(model.FullPath, viewModel, _cacheOptions);
        }
        else
        {
            viewModel.SetModel(model);
        }

            var filteredChildren = new List<FileSystemItemViewModel>();

        foreach (var childModel in model.Children)
        {
            var childViewModel = CreateFromModel(childModel, filterPattern);

            if (string.IsNullOrWhiteSpace(filterPattern))
            {
                filteredChildren.Add(childViewModel);
            }
            else
            {
                if (childModel.Name.Contains(filterPattern, StringComparison.OrdinalIgnoreCase) ||
                    (childViewModel.Children.Count > 0 && childModel.IsDirectory))
                {
                    filteredChildren.Add(childViewModel);
                }
            }
        }

        viewModel.SetChildren(filteredChildren);

        return viewModel;
    }
}
