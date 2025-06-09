using CodePromptus.App.Configuration;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodePromptus.App.Services;

public class FileTreeRepositoryService(IFileSystemService fileSystemService, IGitignoreBuilderService gitignoreBuilderService, IGitIgnoreRecommenderService gitIgnoreRecommenderService)
{
    private readonly IGitignoreBuilderService _gitignoreBuilderService = gitignoreBuilderService;
    private readonly IGitIgnoreRecommenderService _gitIgnoreRecommenderService = gitIgnoreRecommenderService;

    public async Task<(FileSystemItem Root, string GitignorePath)> LoadFileTreeAsync(string rootFolderPath, string gitignorePath)
    {
        if (string.IsNullOrEmpty(rootFolderPath))
        {
            throw new ArgumentException("Root folder path cannot be null or empty.");
        }

        PathIgnorePredicate pathIgnorePredicate;
        if (string.IsNullOrEmpty(gitignorePath))
        {
            var recommendedGitignoreFile = _gitIgnoreRecommenderService.RecommendTemplatesFromFolder(rootFolderPath).First();
            gitignorePath = Path.Combine(Environment.CurrentDirectory, Constants.GitignoreTemplatePath, $"{recommendedGitignoreFile.TemplateName}.gitignore");
            pathIgnorePredicate = await _gitignoreBuilderService.CreateIsIgnoredDelegateAsync(gitignorePath);
        }
        else
        {
            pathIgnorePredicate = await _gitignoreBuilderService.CreateIsIgnoredDelegateAsync(gitignorePath);
        }
        var rootNode = await fileSystemService.LoadDirectoryStructureAsync(rootFolderPath, pathIgnorePredicate);
        var rootModel = Convert(rootNode);
        static FileSystemItem Convert(FileSystemNode node)
        {
            return new FileSystemItem(node.FullPath, node.RelativePath, node.IsDirectory, false)
            {
                Children = [.. node.Children.Select(Convert)]
            };
        }
        return (rootModel, gitignorePath);
    }
}
