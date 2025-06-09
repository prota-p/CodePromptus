using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodePromptus.App.Infrastructure;

public class FileSystemService(ITextEncodingDetectionService encodingDetectionService) : IFileSystemService
{
    public async Task<FileSystemNode> LoadDirectoryStructureAsync(string rootPath, PathIgnorePredicate? shouldIgnore = null)
    {
        var rootNode = new FileSystemNode(rootPath, GetRelativePath(rootPath, rootPath), true);
        await LoadDirectoryContentsAsync(rootPath, rootNode, shouldIgnore);
        return rootNode;
    }

    public async Task<string> ReadFileContentAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        try
        {
            var encoding = encodingDetectionService.DetectEncoding(filePath);
            return await File.ReadAllTextAsync(filePath, encoding);
        }
        catch (Exception ex)
        {
            return $"// Error reading file: {filePath} {ex.Message}";
        }
    }

    public Task<string[]> ReadAllLinesAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return Task.FromResult(Array.Empty<string>());
        }
        try
        {
            var encoding = encodingDetectionService.DetectEncoding(filePath);
            return File.ReadAllLinesAsync(filePath, encoding);
        }
        catch (Exception ex)
        {
            return Task.FromResult(new[] { $"// Error reading file: {filePath}  {ex.Message}" });
        }
    }

    public Task WriteAllTextAsync(string filePath, string content, bool append = false)
    {
        try
        {
            if (append)
            {
                return File.AppendAllTextAsync(filePath, content);
            }
            else
            {
                return File.WriteAllTextAsync(filePath, content);
            }
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public string[] GetDirectories(string path)
    {
        try
        {
            return Directory.GetDirectories(path);
        }
        catch (Exception)
        {
            return [];
        }
    }

    public string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        try
        {
            return Directory.GetFiles(path, searchPattern, searchOption);
        }
        catch (Exception)
        {
            return [];
        }
    }

    public Task<string[]> GetFilesAsync(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        return Task.Run(() => GetFiles(path, searchPattern, searchOption));
    }

    private static string GetRelativePath(string fullPath, string rootPath)
    {
        return Path.GetRelativePath(rootPath, fullPath);
    }

    private static async Task LoadDirectoryContentsAsync(string rootPath, FileSystemNode directoryNode, PathIgnorePredicate? shouldIgnore)
    {
        if (!directoryNode.IsDirectory)
        {
            return;
        }

        try
        {
            var tempChildren = new List<FileSystemNode>();
            var directories = Directory.GetDirectories(directoryNode.FullPath);
            var files = Directory.GetFiles(directoryNode.FullPath);

            // ディレクトリの処理
            foreach (var directory in directories)
            {
                if (shouldIgnore?.Invoke(directory, true) != true)
                {
                    var dirNode = new FileSystemNode(directory, GetRelativePath(directory, rootPath), true);
                    tempChildren.Add(dirNode);
                    await LoadDirectoryContentsAsync(rootPath, dirNode, shouldIgnore);
                }
            }

            // ファイルの処理
            foreach (var file in files)
            {
                if (shouldIgnore?.Invoke(file, false) != true)
                {
                    var fileNode = new FileSystemNode(file, GetRelativePath(file, rootPath), false);
                    tempChildren.Add(fileNode);
                }
            }

            // 子要素をソート（ディレクトリ優先、その後名前順）
            var sortedItems = tempChildren.OrderByDescending(item => item.IsDirectory)
                                         .ThenBy(item => item.Name);

            foreach (var item in sortedItems)
            {
                directoryNode.Children.Add(item);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // アクセス拒否の場合はスキップ
        }
        catch (DirectoryNotFoundException)
        {
            // ディレクトリが見つからない場合はスキップ
        }
    }
}