using System.Collections.ObjectModel;
using System.IO;

namespace CodePromptus.App.Configuration;

public static class Constants
{
    private static readonly string SettingsDirectory = "Settings";
    public static readonly string TemplatePath = Path.Combine(SettingsDirectory, "PromptTemplates", "Prompt.txt");
    public static readonly string GitignoreTemplatePath = Path.Combine(SettingsDirectory, "GitignoreTemplates");
    public static readonly string FolderHistoryPath = Path.Combine(SettingsDirectory, "History", "History.json");
    public static readonly int MaxRecentFolders = 10;
    public static readonly int MaxFileSystemItemViewModelCacheSize = 100000;
}
