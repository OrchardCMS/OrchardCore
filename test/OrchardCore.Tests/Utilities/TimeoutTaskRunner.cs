namespace OrchardCore.Tests.Utilities;

public class TimeoutTaskRunner
{
    public static async Task RunAsync(TimeSpan timeout, Func<Task<bool>> checkNextLoop, string failMessage)
    {
        var timeoutTask = Task.Delay(timeout);
        while (true)
        {
            if (timeoutTask.IsCompleted)
            {
                Assert.Fail(failMessage);
            }
            await Task.Delay(500);
            if (!await checkNextLoop.Invoke())
            {
                break;
            }
        }
    }
}
