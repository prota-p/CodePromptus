using System;
using System.Collections.Generic;
using System.IO;


namespace CodePromptus.App.Models;

public class FileSystemItem(string fullPath, string relativePath, bool isDirectory, bool isSelected)
{
    public string Name { get; init; } = Path.GetFileName(fullPath);
    public string RelativePath { get; init; } = relativePath;
    public string FullPath { get; init; } = fullPath;
    public bool IsDirectory { get; init; } = isDirectory;
    public bool IsSelected { get; set; } = isSelected;
    public List<FileSystemItem> Children { get; init; } = [];

    public IEnumerable<FileSystemItem> EnumerateDescendants()
    {
        yield return this;
        foreach (var child in Children)
        {
            foreach (var descendant in child.EnumerateDescendants())
            {
                yield return descendant;
            }
        }
    }

    public void UpdateSelectedStateToDescendants(List<string> selectedFilePathList)
    {
        HashSet<string> selectedPaths = new(selectedFilePathList, StringComparer.OrdinalIgnoreCase);
        foreach (var item in EnumerateDescendants())
        {
            item.IsSelected = selectedPaths.Contains(item.FullPath);
        }
    }
}