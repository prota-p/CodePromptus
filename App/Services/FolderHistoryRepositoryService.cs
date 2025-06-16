using Avalonia.Controls.Mixins;
using CodePromptus.App.Configuration;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CodePromptus.App.Services;

public class FolderHistoryRepositoryService(IFileSystemService fileSystemService)
{
    private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new() { WriteIndented = true };

    public async Task<FolderHistory> LoadFolderHistoryAsync()
    {
        if (fileSystemService.FileExists(Constants.FolderHistoryPath))
        {
            string file = await fileSystemService.ReadFileContentAsync(Constants.FolderHistoryPath);
            var folderHistory = JsonSerializer.Deserialize<FolderHistory>(file, CachedJsonSerializerOptions);
            if (folderHistory == null)
            {
                throw new InvalidOperationException("Deserialization of FolderHistory returned null.");
            }
            else
            {
                return CreateValidatedHistory(folderHistory);
            }
        }
        else
        {
            return new FolderHistory
            {
                Entries = []
            };
        }
    }

    private FolderHistory CreateValidatedHistory(FolderHistory folderHistory)
    {
        return new()
        {
            Entries = [.. folderHistory.Entries.Where(
                entry => fileSystemService.DirectoryExists(entry.FolderPath) && fileSystemService.FileExists(entry.GitignorePath))]
        };
    }

    public async Task SaveFolderHistoryAsync(FolderHistory folderHistory)
    {
        string json = JsonSerializer.Serialize(folderHistory, CachedJsonSerializerOptions);
        await fileSystemService.WriteAllTextAsync(Constants.FolderHistoryPath, json);
    }
}
