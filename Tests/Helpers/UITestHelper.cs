using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.LogicalTree;

namespace CodePromptus.Tests.Helpers;

internal static class UITestHelper
{
    public static void SaveScreenshot(Window window, string fileName)
    {
        using var bitmap = window.CaptureRenderedFrame();
        string directory = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        Directory.CreateDirectory(directory);
        string filePath = Path.Combine(directory, fileName);
        File.Delete(filePath);
        bitmap?.Save(filePath);
    }

    public static T? GetElementByAutomationId<T>(Control parent, string automationId)
        where T : Control
    {
        return parent.GetLogicalDescendants()
            .OfType<T>()
            .FirstOrDefault(element =>
                element.GetValue(AutomationProperties.AutomationIdProperty)  == automationId);
    }
}
