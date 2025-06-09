using System;
using Serilog;

namespace CodePromptus.App.Infrastructure;

internal static class LoggerConfig
{
    public static readonly ILogger Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Console()
        .WriteTo.File("logs/codepromptus.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();
}

public class SerilogLogService<T> : ILogService<T>
{
    private readonly ILogger _logger = LoggerConfig.Logger.ForContext<T>();

    public void Debug(string messageTemplate, params object?[] args)
        => _logger.Debug(messageTemplate, args);

    public void Info(string messageTemplate, params object?[] args)
        => _logger.Information(messageTemplate, args);

    public void Warning(string messageTemplate, params object?[] args)
        => _logger.Warning(messageTemplate, args);

    public void Warning(Exception? exception, string messageTemplate, params object?[] args)
        => _logger.Warning(exception, messageTemplate, args);

    public void Error(Exception? exception, string messageTemplate, params object?[] args)
        => _logger.Error(exception, messageTemplate, args);
}