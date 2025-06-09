using Avalonia.Headless.XUnit;
using CodePromptus.App.Configuration;
using CodePromptus.App.Models;
using CodePromptus.App.Services;
using CodePromptus.App.ViewModels;
using CodePromptus.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CodePromptus.Tests.ServiceTests;

public class PromptServiceTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public PromptServiceTests()
    {
        _serviceProvider = ServiceBuilder.Build();
        _mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
    }

    [AvaloniaFact]
    public async Task GeneratePrompt_NoFilesSelected_ReturnsNoFilesSelectedMessage()
    {
        // Act
        var promptGeneratorService = _serviceProvider.GetRequiredService<PromptGeneratorService>();
        FileSystemItem dummyRootItem = new(
            Environment.CurrentDirectory,
            ".",
            true,
            true
        );
        var dummyGeneratedPrompt = await promptGeneratorService.GeneratePromptAsync([dummyRootItem]);
        Assert.Contains("No files selected.", dummyGeneratedPrompt);
    }

    public void Dispose()
    {
        DisposeHelper.DisposeMainWindowViewModelAndServProvider(_mainWindowViewModel, _serviceProvider);
        GC.SuppressFinalize(this);
    }
}