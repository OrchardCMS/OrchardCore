using OrchardCore.Jobs;

namespace OrchardCore.Tests.Modules.OrchardCore.Jobs;

/// <summary>
/// What a run of <see cref="TestJob"/> does. Each test uses a key of its own, to read what its runs did.
/// </summary>
public sealed class TestJobParameters
{
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the number of executions that fail before one succeeds.
    /// </summary>
    public int FailCount { get; set; }

    public int DelayMilliseconds { get; set; }

    public bool WaitForCancellation { get; set; }

    public int Children { get; set; }

    public int FailingChildren { get; set; }
}

public sealed class TestJobState
{
    public int Executions;
    public int Concurrent;
    public int MaxConcurrent;
    public int Canceled;
}

/// <summary>
/// A job for the tests of the jobs engine, registered for every tenant of the test site. It counts its executions
/// by key, and fails, waits or queues child runs as its parameters say.
/// </summary>
public sealed class TestJob : IJob
{
    public const string Name = "TestJob";

    public const string DeclaredScheduleName = "TestDeclaredScheduleJob";

    public const string TransientName = "TestTransientJob";

    public const string ManualName = "TestManualJob";

    public static readonly ConcurrentDictionary<string, TestJobState> States = new(StringComparer.Ordinal);

    public static TestJobState GetState(string key) => States.GetOrAdd(key, _ => new TestJobState());

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var parameters = context.GetParameters<TestJobParameters>();
        var state = GetState(parameters.Key);
        var executions = Interlocked.Increment(ref state.Executions);
        var concurrent = Interlocked.Increment(ref state.Concurrent);

        int max;
        while (concurrent > (max = Volatile.Read(ref state.MaxConcurrent)))
        {
            Interlocked.CompareExchange(ref state.MaxConcurrent, concurrent, max);
        }

        try
        {
            context.LogInformation($"Execution {executions}");
            context.ReportProgress(50, "Half way");

            if (parameters.DelayMilliseconds > 0)
            {
                await Task.Delay(parameters.DelayMilliseconds, cancellationToken);
            }

            if (parameters.WaitForCancellation)
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }

            if (executions <= parameters.FailCount)
            {
                throw new InvalidOperationException($"Test failure {executions}");
            }

            for (var i = 0; i < parameters.Children; i++)
            {
                await context.EnqueueChildAsync(Name, new TestJobParameters
                {
                    Key = $"{parameters.Key}/{i}",
                    FailCount = i < parameters.FailingChildren ? 1 : 0,
                });
            }

            context.SetResult(new TestJobResult { Executions = executions });
        }
        catch (OperationCanceledException)
        {
            Interlocked.Increment(ref state.Canceled);

            throw;
        }
        finally
        {
            Interlocked.Decrement(ref state.Concurrent);
        }
    }
}

public sealed class TestJobResult
{
    public int Executions { get; set; }
}
