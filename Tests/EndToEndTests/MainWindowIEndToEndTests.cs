using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using CodePromptus.App.Configuration;
using CodePromptus.App.Models;
using CodePromptus.App.Services;
using CodePromptus.App.ViewModels;
using CodePromptus.App.Views;
using CodePromptus.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace CodePromptus.Tests.EndToEndTests;

public class MainWindowIEndtoEndTests : IDisposable
{
    private readonly MainWindow _mainWindow;
    private readonly IServiceProvider _serviceProvider;
    private readonly MainWindowViewModel _mainWindowViewModel;

    public MainWindowIEndtoEndTests()
    {
        // Arrange
        _serviceProvider = ServiceBuilder.Build();
        _mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        _mainWindow = new MainWindow
        {
            DataContext = _mainWindowViewModel
        };
        File.Delete(Constants.FolderHistoryPath);
    }

    [AvaloniaFact]
    public async Task ProjectTree_LoadAndFileSelect_Should_ReflectInPromptAndGitignorePath()
    {        
        // Act
        _mainWindow.Show();
        UITestHelper.SaveScreenshot(_mainWindow, "before_act.png");

        var testRootPath = Path.Combine("TestData","SampleProject");
        Assert.True(Directory.Exists(testRootPath), $"Test data directory '{testRootPath}' does not exist.");
        await _mainWindowViewModel.FileTreeVm.LoadFileSystemAsync(testRootPath, string.Empty);

        // Assert
        await AssertHelper.AssertEventuallyAsync(() =>
        {
            var checkBox = UITestHelper.GetElementByAutomationId<CheckBox>(_mainWindow, ".");
            Assert.NotNull(checkBox);
            checkBox.IsChecked = true;
        });
        var fileTreeVm = _mainWindowViewModel.FileTreeVm;
        Assert.Equal(Path.Combine(Environment.CurrentDirectory, Constants.GitignoreTemplatePath, "VisualStudio.gitignore"), fileTreeVm.GitignorePath);
        Assert.Equal(2, fileTreeVm.RootItemVms[0].GetAllDescendants().Count());

        const string ExpectedContent = @"Console.WriteLine(""Hello, World!"");";
        await AssertHelper.AssertEventuallyAsync(() =>
        { 
            var textBox = UITestHelper.GetElementByAutomationId<TextBox>(_mainWindow, UIAutomationIds.GeneratedPrompt);
            Assert.NotNull(textBox);
            string? generatedPrompt = textBox.Text;
            Assert.Contains(ExpectedContent, generatedPrompt);
        });

        UITestHelper.SaveScreenshot(_mainWindow, "after_act.png");
    }

    public void Dispose()
    {
        DisposeHelper.DisposeMainWindowViewModelAndServProvider(_mainWindowViewModel, _serviceProvider);
        GC.SuppressFinalize(this);
    }
}