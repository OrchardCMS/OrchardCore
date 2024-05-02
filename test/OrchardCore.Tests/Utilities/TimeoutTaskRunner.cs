namespace OrchardCore.Tests.Utilities;

public class TimeoutTaskRunner
{
    public static async Task RunAsync(TimeoutTaskOption option)
    {
        var timeoutTask = Task.Delay(option.Timeout);
        while (true)
        {
            if (timeoutTask.IsCompleted)
            {
                Assert.Fail(option.TimeoutMessage);
            }
            await Task.Delay(option.Interval);
            if (!await option.CanNextLoop.Invoke())
            {
                break;
            }
        }
    }
}

public class TimeoutTaskOption
{
    public Func<Task<bool>> CanNextLoop { get; set; }
    public string TimeoutMessage { get; set; }
    public TimeSpan Timeout { get; set; }
    public int Interval { get; set; } = 500;
}
