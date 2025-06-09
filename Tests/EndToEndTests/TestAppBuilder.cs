using Avalonia.Headless;
using Avalonia;
using CodePromptus.Tests.EndToEndTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace CodePromptus.Tests.EndToEndTests;
public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
        .UseSkia()  
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        });
    }
}