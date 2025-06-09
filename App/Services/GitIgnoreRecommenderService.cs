using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodePromptus.App.Infrastructure;

namespace CodePromptus.App.Services;

/// <summary>
/// Suggests suitable .gitignore templates for a project by sampling its top‑level files/folders
/// and a shallow search for well‑known build / project files.
/// 
/// Note: This uses a naive scoring algorithm based on simple pattern matching and may not
/// accurately detect complex project structures or mixed-language projects.
/// </summary>
public sealed class GitIgnoreRecommenderService(IFileSystemService fs) : IGitIgnoreRecommenderService
{
    private readonly Dictionary<string, int> _scores = new(StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, LanguageConfig> Lang = new Dictionary<string, LanguageConfig>(StringComparer.OrdinalIgnoreCase)
    {
        ["VisualStudio"] = new([".csproj", ".sln", "web.config", "app.config"], [".cs", ".vb", ".fs"], ["bin", "obj"]),
        ["Node"] = new(["package.json", "tsconfig.json"], [".js", ".ts"], ["node_modules", "dist"]),
        ["Python"] = new(["requirements.txt", "setup.py"], [".py"], ["__pycache__", "venv"]),
        ["Java"] = new(["pom.xml", "build.gradle"], [".java", ".class"], ["target", "build"]),
        ["Maven"] = new(["pom.xml"], [], []),
        ["Gradle"] = new(["build.gradle"], [], ["build"]),
        ["Go"] = new(["go.mod"], [".go"], []),
        ["Rust"] = new(["cargo.toml"], [".rs"], []),
        ["Ruby"] = new(["Gemfile"], [".rb"], ["vendor"]),
        ["Flutter"] = new(["pubspec.yaml"], [], []),
        ["Dart"] = new(["pubspec.yaml"], [".dart"], []),
        ["Laravel"] = new(["artisan", ".env"], [], ["app"]),
        ["Unity"] = new([], [], ["assets", "projectsettings"]),
        ["C"] = new(["makefile", "cmakelists.txt"], [".c", ".h"], []),
        ["C++"] = new(["makefile", "cmakelists.txt"], [".cpp", ".hpp"], [])
    };

    private static readonly string[] ProjectPatterns =
    [
        "*.csproj", "*.sln", "package.json", "pom.xml", "build.gradle", "Makefile", "CMakeLists.txt",
        "setup.py", "requirements.txt", "Gemfile", "Cargo.toml", "pubspec.yaml", "*.xcodeproj",
        "web.config", "tsconfig.json", ".env", "go.mod", "artisan"
    ];

    public IEnumerable<GitIgnoreRecommendation> RecommendTemplatesFromFolder(string root, int limit)
    {
        if (!fs.DirectoryExists(root))
            throw new DirectoryNotFoundException(root);

        _scores.Clear();
        var (folders, files) = GetTopLevelItems(root);

        var projectFiles = FindProjectFiles(root).Concat(files);
        EvaluateFiles(projectFiles, isProject: true);
        EvaluateFolders(folders);
        EvaluateFiles(files, isProject: false);

        return BuildResult(limit);
    }

    #region helpers
    private List<string> FindProjectFiles(string path)
    {
        var found = new List<string>();
        Add(path);
        if (found.Count == 0)
            foreach (var dir in fs.GetDirectories(path))
                Add(dir);
        return found;

        void Add(string dir)
        {
            foreach (var pattern in ProjectPatterns)
                found.AddRange(fs.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly));
        }
    }

    private static (IEnumerable<string> folders, IEnumerable<string> files) GetTopLevelItems(string root)
    {
        static IEnumerable<string> Names(IEnumerable<string> paths) =>
            paths.Select(Path.GetFileName)
                 .Where(n => n != null)
                 .Cast<string>();
        return (Names(Directory.GetDirectories(root)), Names(Directory.GetFiles(root)));
    }

    private void EvaluateFiles(IEnumerable<string> files, bool isProject)
    {
        foreach (var file in files)
        {
            var name = Path.GetFileName(file)!.ToLowerInvariant();
            var ext = Path.GetExtension(name);

            foreach (var (lang, cfg) in Lang)
            {
                var score = (isProject, cfg.ProjectFiles.Any(p => name.Contains(p, StringComparison.OrdinalIgnoreCase))) switch
                {
                    (true, true) => 10,
                    (false, _) when cfg.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase) => 2,
                    _ => 0
                };
                if (score > 0) _scores[lang] = _scores.GetValueOrDefault(lang) + score;
            }
        }
    }

    private void EvaluateFolders(IEnumerable<string> folders)
    {
        var list = folders.Select(f => f.ToLowerInvariant()).ToList();
        foreach (var folder in list)
            foreach (var (lang, cfg) in Lang)
                if (cfg.Folders.Contains(folder, StringComparer.OrdinalIgnoreCase))
                    _scores[lang] = _scores.GetValueOrDefault(lang) + 5;

        if (list.Contains("assets") && list.Contains("projectsettings"))
            _scores["Unity"] = _scores.GetValueOrDefault("Unity") + 10;
    }

    private IEnumerable<GitIgnoreRecommendation> BuildResult(int limit)
    {
        if (_scores.Count == 0) return DefaultRecommendations;

        var max = _scores.Values.Max();
        return _scores.OrderByDescending(kv => kv.Value)
                      .Take(limit)
                      .Select(kv => new GitIgnoreRecommendation(kv.Key,
                          Math.Clamp((int)(kv.Value / (double)max * 100), 0, 100),
                          $"https://raw.githubusercontent.com/github/gitignore/main/{kv.Key}.gitignore"));
    }

    private static IEnumerable<GitIgnoreRecommendation> DefaultRecommendations =>
    [
        new GitIgnoreRecommendation("VisualStudio", 60, "https://raw.githubusercontent.com/github/gitignore/main/VisualStudio.gitignore"),
        new GitIgnoreRecommendation("Node",         30, "https://raw.githubusercontent.com/github/gitignore/main/Node.gitignore"),
        new GitIgnoreRecommendation("Python",       30, "https://raw.githubusercontent.com/github/gitignore/main/Python.gitignore")
    ];
    #endregion
}

public sealed record LanguageConfig(IReadOnlyList<string> ProjectFiles, IReadOnlyList<string> Extensions, IReadOnlyList<string> Folders);

public sealed record GitIgnoreRecommendation(string TemplateName, int ConfidenceScore, string GitIgnoreUrl);
