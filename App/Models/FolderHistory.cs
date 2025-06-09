using CodePromptus.App.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodePromptus.App.Models;

public class FolderHistory
{
    public required List<FolderHistoryEntry> Entries { get; init; }

    public FolderHistoryEntry? GetEntry(FileSystemItem root)
    {
        var entry = Entries.FirstOrDefault(entry => entry.FolderPath.Equals(root.FullPath, StringComparison.OrdinalIgnoreCase));
        entry?.UpdateLastAccessed();
        return entry;
    }

    public FolderHistoryEntry GetOrCreateEntry(FileSystemItem root, string gitignorePath)
    {
        var entry = GetEntry(root);
        if (entry == null)
        {
            entry = CreateNewEntry(root, gitignorePath);
            Entries.Add(entry);
        }
        else
        {
            entry.UpdateGitignorePath(gitignorePath);
        }
        SortEntries();
        return entry;
    }

    public FolderHistoryEntry? GetLatestEntry() => Entries.Count > 0 ? Entries[0] : null;

    private static FolderHistoryEntry CreateNewEntry(FileSystemItem root, string gitignorePath)
    {
        var selectedFilePathList = root.EnumerateDescendants().Where(item => item.IsSelected).Select(item => item.FullPath).ToList();
        return new FolderHistoryEntry(root.FullPath, gitignorePath, selectedFilePathList, DateTime.Now);
    }

    private void SortEntries()
    {
        var ordered = Entries
            .OrderByDescending(entry => entry.LastAccessed)
            .Take(Constants.MaxRecentFolders)
            .ToList();
        Entries.Clear();
        Entries.AddRange(ordered);
    }
}

public record FolderHistoryEntry(
    string FolderPath,
    string GitignorePath,
    List<string> SelectedFilePathList,
    DateTime LastAccessed
)
{
    public string GitignorePath { get; set; } = GitignorePath;
    public DateTime LastAccessed { get; private set; } = LastAccessed;

    public string DisplayName => $"{FolderPath}[{LastAccessed}]";

    public void UpdateLastAccessed() => LastAccessed = DateTime.Now;
    public void UpdateGitignorePath(string gitignorePath)
    {
        GitignorePath = gitignorePath;
    }

    public void UpdateSelectedState(FileSystemItem root)
    {
        SelectedFilePathList.Clear();
        SelectedFilePathList.AddRange(root.EnumerateDescendants().Where(item => item.IsSelected).Select(item => item.FullPath));
    }

    public void RemoveNonExistentPathsFromSelection(FileSystemItem root)
    {
        HashSet<string> selectedPathsSet = new(SelectedFilePathList, StringComparer.OrdinalIgnoreCase);
        SelectedFilePathList.Clear();
        foreach (var item in root.EnumerateDescendants())
        {
            if (selectedPathsSet.Contains(item.FullPath))
            {
                SelectedFilePathList.Add(item.FullPath);
            }
        }
    }
}