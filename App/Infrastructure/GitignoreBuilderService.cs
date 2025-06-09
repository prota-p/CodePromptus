using System.IO;
using System.Threading.Tasks;

namespace CodePromptus.App.Infrastructure;
public class GitignoreBuilderService(IFileSystemService fileSystemService) : IGitignoreBuilderService
{
    public async Task<PathIgnorePredicate> CreateIsIgnoredDelegateAsync(string gitignorePath)
    {
        if (!File.Exists(gitignorePath))
            throw new FileNotFoundException($".gitignore file not found at path: {gitignorePath}");
        var ignoreParser = new Ignore.Ignore();
        var gitignoreLines = await fileSystemService.ReadAllLinesAsync(gitignorePath);
        foreach (var line in gitignoreLines)
        {
            ignoreParser.Add(line);
        }
        // Add default gitignore rules
        ignoreParser.Add(".git");
        ignoreParser.Add(".git/");
      
        return (path, isDirectory) =>
        {
            string actualPath = path.Replace('\\', '/');
            if (isDirectory)
            {

                actualPath = actualPath.TrimEnd('/') + "/";
            }
            return ignoreParser.IsIgnored(actualPath);
        };
    }
}