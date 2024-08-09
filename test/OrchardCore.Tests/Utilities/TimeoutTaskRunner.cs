namespace OrchardCore.Tests.Utilities;

public class TimeoutTaskRunner
{
    public static async Task RunAsync(TimeSpan timeout, Func<Task<bool>> checkNextLoop, string failMessage, int interval = 500)
    {
        var timeoutTask = Task.Delay(timeout);
        while (true)
        {
            if (timeoutTask.IsCompleted)
            {
                Assert.Fail(failMessage);
            }
            await Task.Delay(interval);
            if (!await checkNextLoop.Invoke())
            {
                break;
            }
        }
    }
}
