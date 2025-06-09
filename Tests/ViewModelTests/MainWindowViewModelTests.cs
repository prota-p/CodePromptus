using CodePromptus.App.Configuration;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Services;
using CodePromptus.App.ViewModels;
using CodePromptus.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Reactive.Linq;
using System.Text;

namespace CodePromptus.Tests.ViewModelTests;

public class MainWindowViewModelTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly Mock<IFileSystemService> _mockFileSystemService = new();
    private readonly Mock<IGitignoreBuilderService> _mockGitignoreBuilderService = new();
    private readonly Mock<ITextEncodingDetectionService> _mockTextEncodingDetectionService = new();
    private readonly Mock<IGitIgnoreRecommenderService> _mockGitIgnoreRecommenderService = new();

    public MainWindowViewModelTests()
    {
        _mockTextEncodingDetectionService.Setup(s => s.DetectEncoding(It.IsAny<string>())).Returns(Encoding.UTF8);
        _mockGitignoreBuilderService.Setup(s => s.CreateIsIgnoredDelegateAsync(It.IsAny<string>())).ReturnsAsync((path, isDir) => false);
        _mockGitIgnoreRecommenderService.Setup(s => s.RecommendTemplatesFromFolder(It.IsAny<string>(), It.IsAny<int>()))
            .Returns( [ new GitIgnoreRecommendation("VisualStudio", 60, "https://raw.githubusercontent.com/github/gitignore/main/VisualStudio.gitignore") ]);

        _serviceProvider = ServiceBuilder.Build(services =>
        {
            services.AddSingleton(_mockFileSystemService.Object);
            services.AddSingleton(_mockGitignoreBuilderService.Object);
            services.AddSingleton(_mockTextEncodingDetectionService.Object);
            services.AddSingleton(_mockGitIgnoreRecommenderService.Object);
        });

        _mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
    }

    [Fact]
    public async Task DirectorySelection_UpdatesSelectedFiles_AndPromptContent()
    {
        // Arrange - Test data setup
        var testRootPath = @"C\TestDir";
        const string subDirName = "SubDir";
        const string testFile1Name = "file1.cs";
        const string testFile2Name = "file2.cs";

        var subDirPath = Path.Combine(testRootPath, subDirName);
        var testFile1Path = Path.Combine(subDirPath, testFile1Name);
        var testFile2Path = Path.Combine(testRootPath, testFile2Name);
        var testFile1RelativePath = Path.Combine(subDirName, testFile1Name);

        const string testFile1Content = "file1-content";
        const string testFile2Content = "file2-content";

        string templateContent = """
        Project Hierarchy:
        {{ hierarchy }}
        {% if has_selected_files %}
        {% for file in selected_files %}
        File: {{ file.Path }}
        Code Content:
        {{ file.Content }}
        --------------------------------------------------
        {% endfor %}
        {% else %}
        No files selected. Select files from the tree view to include their content in the prompt.
        {% endif %}
        """;

        // Arrange - File system structure setup
        var fileNode1 = new FileSystemNode(testFile1Path, testFile1RelativePath, false);
        var fileNode2 = new FileSystemNode(testFile2Path, testFile2Name, false);

        var subDirNode = new FileSystemNode(subDirPath, subDirName, true);
        subDirNode.Children.Add(fileNode1);

        var rootNode = new FileSystemNode(testRootPath, Path.GetFileName(testRootPath), true);
        rootNode.Children.Add(subDirNode);
        rootNode.Children.Add(fileNode2);

        // Arrange - Mock service setup
        _mockFileSystemService.Setup(fs => fs.LoadDirectoryStructureAsync(testRootPath, It.IsAny<PathIgnorePredicate>()))
                              .ReturnsAsync(rootNode);
        _mockFileSystemService.Setup(fs => fs.FileExists(Constants.FolderHistoryPath))
                              .Returns(false);
        _mockFileSystemService.Setup(fs => fs.FileExists(Constants.TemplatePath))
                              .Returns(true);
        _mockFileSystemService.Setup(fs => fs.ReadFileContentAsync(Constants.TemplatePath))
                              .ReturnsAsync(templateContent);
        _mockFileSystemService.Setup(fs => fs.ReadFileContentAsync(testFile1Path))
                              .ReturnsAsync(testFile1Content);
        _mockFileSystemService.Setup(fs => fs.ReadFileContentAsync(testFile2Path))
                              .ReturnsAsync(testFile2Content);

        // Act
        await _mainWindowViewModel.FileTreeVm.LoadFileSystemAsync(testRootPath, string.Empty);
        _mainWindowViewModel.FileTreeVm.RootItemVms[0].Children.Where(x => x.Name == subDirName).First().IsSelected = true;

        // Assert
        await AssertHelper.AssertEventuallyAsync(() =>
        {
            string generatedPrompt = _mainWindowViewModel.PromptDisplayVm.GeneratedPrompt;
            Assert.Contains(testFile1Content, generatedPrompt);
            Assert.DoesNotContain(testFile2Content, generatedPrompt);
        });
        Assert.Single(_mainWindowViewModel.SelectedFilesVm.SelectedItems);
        Assert.Contains(_mainWindowViewModel.SelectedFilesVm.SelectedItems, item => item.Name == testFile1Name);
    }

    public void Dispose()
    {
        DisposeHelper.DisposeMainWindowViewModelAndServProvider(_mainWindowViewModel, _serviceProvider);
        GC.SuppressFinalize(this);
    }
}