namespace CodePromptus.Tests.Helpers;

internal class AssertHelper
{
    public static async Task AssertEventuallyAsync(
            Action assertion,
            int timeoutMilliseconds = 2000,
            int intervalMilliseconds = 200,
            string? failureMessage = null)
    {
        var start = DateTime.UtcNow;
        Exception? lastException = null;
        while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMilliseconds)
        {
            try
            {
                assertion();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
            await Task.Delay(intervalMilliseconds);
        }
        if (lastException != null)
            throw lastException;
        throw new TimeoutException(failureMessage ?? "AssertEventuallyAsync: Time out");
    }
}
