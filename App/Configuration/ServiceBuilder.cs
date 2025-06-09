using CodePromptus.App.Infrastructure;
using CodePromptus.App.Services;
using CodePromptus.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CodePromptus.App.Configuration;

public static class ServiceBuilder
{
    public static IServiceProvider Build(Action<ServiceCollection>? overrideAction = null)
    {
        var services = new ServiceCollection();

        //Infrastructure Services
        services.AddSingleton(typeof(ILogService<>), typeof(SerilogLogService<>));
        services.AddSingleton<ITextEncodingDetectionService, UtfUnknownTextEncodingDetectionService>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IGitignoreBuilderService, GitignoreBuilderService>();

        //App Services
        services.AddSingleton<ToastService>();
        services.AddSingleton<IGitIgnoreRecommenderService, GitIgnoreRecommenderService>();
        services.AddSingleton<FileTreeRepositoryService>();
        services.AddSingleton<FolderHistoryRepositoryService>();
        services.AddSingleton<PromptGeneratorService>();

        //View Models
        services.AddSingleton<FileSystemItemViewModelFactory>();
        services.AddTransient<FileTreeViewModel>();
        services.AddTransient<SelectedFilesViewModel>();
        services.AddTransient<PromptDisplayViewModel>();
        services.AddTransient<ToastControllerViewModel>();
        services.AddTransient<MainWindowViewModel>();

        overrideAction?.Invoke(services);

        return services.BuildServiceProvider();
    }
}
