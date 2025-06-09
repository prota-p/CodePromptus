using System;

namespace CodePromptus.App.Infrastructure;

public interface ILogService<T>
{
    void Debug(string messageTemplate, params object?[] args);
    void Info(string messageTemplate, params object?[] args);
    void Warning(string messageTemplate, params object?[] args);
    void Warning(Exception? exception, string messageTemplate, params object?[] args);
    void Error(Exception? exception, string messageTemplate, params object?[] args);
}