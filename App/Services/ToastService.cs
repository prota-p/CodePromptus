using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CodePromptus.App.Services;

public class ToastService
{
    private readonly Subject<ToastNotification> _notificationsSubject = new();

    public IObservable<ToastNotification> Notifications => _notificationsSubject.AsObservable();

    public void ShowToast(string message, ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)
    {
        var notification = new ToastNotification(message, level, duration);
        _notificationsSubject.OnNext(notification);
    }

    public void ShowInfo(string message, TimeSpan? duration = null)
        => ShowToast(message, ToastLevel.Info, duration);

    public void ShowSuccess(string message, TimeSpan? duration = null)
        => ShowToast(message, ToastLevel.Success, duration);

    public void ShowWarning(string message, TimeSpan? duration = null)
        => ShowToast(message, ToastLevel.Warning, duration);

    public void ShowError(string message, TimeSpan? duration = null)
        => ShowToast(message, ToastLevel.Error, duration);
}

public enum ToastLevel
{
    Info,
    Success,
    Warning,
    Error
}

public class ToastNotification(string message, ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)
{
    public string Message { get; } = message;
    public ToastLevel Level { get; } = level;
    public TimeSpan Duration { get; } = duration ?? TimeSpan.FromSeconds(3);
}