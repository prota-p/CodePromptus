using Avalonia.Controls;
using System;

namespace CodePromptus.App.Helpers;

public static class DesignModeGuard
{
    public static void ThrowIfNotDesignMode(string? message = null)
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException(
                message ?? "The parameterless constructor is allowed for design-time use only."
            );
        }
    }
}