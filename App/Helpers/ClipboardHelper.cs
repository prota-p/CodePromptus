using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace CodePromptus.App.Helpers;

public static class ClipboardHelper
{
    public static async Task CopyToClipboardAsync(string text)
    {
        var clipboard = GetClipboard();
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(text);
        }
    }

    private static IClipboard? GetClipboard()
    {
        // For desktop applications
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow != null)
        {
            return desktop.MainWindow.Clipboard;
        }
        // For mobile applications (if needed)
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView &&
                 singleView.MainView is TopLevel topLevel)
        {
            return topLevel.Clipboard;
        }
        return null;
    }
}