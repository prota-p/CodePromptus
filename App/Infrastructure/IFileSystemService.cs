using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CodePromptus.App.Infrastructure;

public interface IFileSystemService
{
    Task<FileSystemNode> LoadDirectoryStructureAsync(string rootPath, PathIgnorePredicate? shouldIgnore = null);
    Task<string> ReadFileContentAsync(string filePath);
    Task WriteAllTextAsync(string filePath, string content, bool append = false);
    Task<string[]> ReadAllLinesAsync(string filePath);
    bool DirectoryExists(string path);
    bool FileExists(string path);
    string[] GetDirectories(string path);
    string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);
}


public record FileSystemNode(string FullPath, string RelativePath, bool IsDirectory)
{
    public string Name => Path.GetFileName(FullPath);
    public List<FileSystemNode> Children { get; } = [];
}

public delegate bool PathIgnorePredicate(string path, bool isDirectory);