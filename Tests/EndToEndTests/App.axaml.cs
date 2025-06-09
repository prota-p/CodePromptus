using Avalonia;
using Avalonia.Markup.Xaml;

namespace CodePromptus.Tests.EndToEndTests;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}