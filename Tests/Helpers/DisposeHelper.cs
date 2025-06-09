using CodePromptus.App.ViewModels;

namespace CodePromptus.Tests.Helpers;

internal static class DisposeHelper
{
    public static void DisposeMainWindowViewModelAndServProvider(
        MainWindowViewModel mainWindowViewModel, 
        IServiceProvider serviceProvider)
    {
        mainWindowViewModel.Dispose();
        if (serviceProvider is IDisposable disposableServiceProvider)
        {
            disposableServiceProvider.Dispose();
        }
    }   
}
