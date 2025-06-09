using CodePromptus.App.Configuration;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Models;
using System;
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
            return folderHistory ?? throw new InvalidOperationException("Failed to deserialize FolderHistory from file.");
        }
        else
        {
            return new FolderHistory
            {
                Entries = []
            };
        }
    }

    public async Task SaveFolderHistoryAsync(FolderHistory folderHistory)
    {
        string json = JsonSerializer.Serialize(folderHistory, CachedJsonSerializerOptions);
        await fileSystemService.WriteAllTextAsync(Constants.FolderHistoryPath, json);
    }
}
