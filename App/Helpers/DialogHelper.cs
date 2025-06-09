using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodePromptus.App.Helpers;

public static class DialogHelper
{
    public static async Task<string?> ShowOpenFileAsync(Window window, string title, string[] fileExtensions)
    {
        var filters = new List<FilePickerFileType>();

        if (fileExtensions != null && fileExtensions.Length > 0)
        {
            filters.Add(new FilePickerFileType(title)
            {
                Patterns = [.. fileExtensions.Select(ext => $"*.{ext}")]
            });
        }

        var result = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = filters.Count > 0 ? filters : null
        });

        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }

    public static async Task<string?> ShowOpenFolderAsync(Window window, string title)
    {
        var result = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        // Use indexing instead of FirstOrDefault
        return result.Count > 0 ? result[0].Path.LocalPath : null;
    }
}
