using CodePromptus.App.Helpers;
using CodePromptus.App.Infrastructure;
using CodePromptus.App.Services;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CodePromptus.App.ViewModels;

public class ToastControllerViewModel : ViewModelBase, IDisposable
{
    private readonly ToastService _toastService;
    private readonly CompositeDisposable _disposables = [];
    private readonly ILogService<ToastControllerViewModel> _logger;

    public ObservableCollection<ToastItemViewModel> Toasts { get; } = [];

#pragma warning disable CS8618
    public ToastControllerViewModel() { DesignModeGuard.ThrowIfNotDesignMode(); } // For design-time data
#pragma warning restore CS8618

    public ToastControllerViewModel(ToastService toastService, ILogService<ToastControllerViewModel> logger)
    {
        _toastService = toastService;
        _logger = logger;

        _toastService.Notifications
            .Subscribe(notification => _ = ShowToastAsync(notification))
            .DisposeWith(_disposables);
    }

    private async Task ShowToastAsync(ToastNotification notification)
    {
        var toastVm = new ToastItemViewModel(notification);

        Toasts.Add(toastVm);

        try
        {
            await Task.Delay(toastVm.Duration, toastVm.CancellationToken);
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, likely due to user interaction
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred while displaying toast: {Message}", notification.Message);
        }

        toastVm.IsVisible = false;
        await Task.Delay(300);  // Allow time for fade-out animation
        Toasts.Remove(toastVm);
    }


    public void Dispose()
    {
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}