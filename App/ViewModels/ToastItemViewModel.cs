using CodePromptus.App.Helpers;
using CodePromptus.App.Services;
using ReactiveUI.Fody.Helpers;
using System;
using System.Threading;

namespace CodePromptus.App.ViewModels;

public class ToastItemViewModel : ViewModelBase
{
    private readonly CancellationTokenSource _cts = new();
    private readonly ToastNotification _notification;

    [Reactive] public bool IsVisible { get; set; }
    public string Message => _notification.Message;
    public ToastLevel Level => _notification.Level;
    public TimeSpan Duration => _notification.Duration;

#pragma warning disable CS8618
    public ToastItemViewModel() { DesignModeGuard.ThrowIfNotDesignMode(); } // For design-time data
#pragma warning restore CS8618

    public ToastItemViewModel(ToastNotification notification)
    {
        _notification = notification;
        IsVisible = true;
    }

    public void Close()
    {
        IsVisible = false;
        _cts.Cancel();
    }

    public CancellationToken CancellationToken => _cts.Token;
}