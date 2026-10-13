using OrchardCore.BackgroundTasks;

namespace OrchardCore.Tests.BackgroundTasks;

public class BackgroundTaskMonitorTests
{
    private static readonly DateTime _referenceTime = new(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task GetStatesAsync_SeveralTenants_ReturnsOnlyStatesOfTenant()
    {
        var monitor = new BackgroundTaskMonitor();
        AddScheduler(monitor, "Tenant1", "Task1");
        AddScheduler(monitor, "Tenant1", "Task2");
        AddScheduler(monitor, "Tenant2", "Task1");

        var states = await monitor.GetStatesAsync("Tenant1");

        Assert.Equal(["Task1", "Task2"], states.Select(state => state.Name).Order());
        Assert.All(states, state => Assert.Equal("Tenant1", state.Tenant));
    }

    [Fact]
    public async Task GetStatesAsync_UnknownTenant_ReturnsEmpty()
    {
        var monitor = new BackgroundTaskMonitor();
        AddScheduler(monitor, "Tenant1", "Task1");

        Assert.Empty(await monitor.GetStatesAsync("Tenant2"));
    }

    [Fact]
    public async Task GetStateAsync_KnownTask_ReturnsState()
    {
        var monitor = new BackgroundTaskMonitor();
        var scheduler = AddScheduler(monitor, "Tenant1", "Task1");
        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Succeeded);

        var state = await monitor.GetStateAsync("Tenant1", "Task1");

        Assert.NotNull(state);
        Assert.Equal(1, state.RunCount);
    }

    [Theory]
    [InlineData("Tenant1", "Unknown")]
    [InlineData("Tenant2", "Task1")]
    [InlineData("Tenant", "1Task1")]
    [InlineData(null, "Task1")]
    [InlineData("Tenant1", null)]
    public async Task GetStateAsync_UnknownTask_ReturnsNull(string tenant, string name)
    {
        var monitor = new BackgroundTaskMonitor();
        AddScheduler(monitor, "Tenant1", "Task1");

        Assert.Null(await monitor.GetStateAsync(tenant, name));
    }

    [Fact]
    public async Task RequestRunAsync_UnknownTask_ReturnsNotFoundAndSignalsRunRequested()
    {
        var monitor = new BackgroundTaskMonitor();
        AddScheduler(monitor, "Tenant1", "Task1");

        var result = await monitor.RequestRunAsync("Tenant2", "Task1");

        Assert.Equal(BackgroundTaskRunRequestResult.NotFound, result);

        // The background service is triggered to load the tasks of a tenant that just started.
        Assert.True(monitor.RunRequested.IsCompleted);
    }

    [Fact]
    public async Task RequestRunAsync_IdleTask_QueuesItAndSignalsRunRequested()
    {
        var monitor = new BackgroundTaskMonitor();
        var scheduler = AddScheduler(monitor, "Tenant1", "Task1");
        var runRequested = monitor.RunRequested;

        var result = await monitor.RequestRunAsync("Tenant1", "Task1");

        Assert.Equal(BackgroundTaskRunRequestResult.Queued, result);
        Assert.Equal(BackgroundTaskStatus.Queued, scheduler.GetState().Status);
        Assert.True(runRequested.IsCompleted);
    }

    [Fact]
    public async Task RequestRunAsync_RunningTask_DoesNotSignalRunRequested()
    {
        var monitor = new BackgroundTaskMonitor();
        var scheduler = AddScheduler(monitor, "Tenant1", "Task1");
        scheduler.Run();

        var result = await monitor.RequestRunAsync("Tenant1", "Task1");

        Assert.Equal(BackgroundTaskRunRequestResult.AlreadyRunning, result);
        Assert.False(monitor.RunRequested.IsCompleted);
    }

    [Fact]
    public async Task RequestRunAsync_QueuedTask_ReturnsAlreadyQueued()
    {
        var monitor = new BackgroundTaskMonitor();
        AddScheduler(monitor, "Tenant1", "Task1");
        await monitor.RequestRunAsync("Tenant1", "Task1");

        var result = await monitor.RequestRunAsync("Tenant1", "Task1");

        Assert.Equal(BackgroundTaskRunRequestResult.AlreadyQueued, result);
    }

    [Fact]
    public async Task ResetRunRequested_AfterRequest_WaitsForNextRequest()
    {
        var monitor = new BackgroundTaskMonitor();
        AddScheduler(monitor, "Tenant1", "Task1");
        AddScheduler(monitor, "Tenant1", "Task2");
        await monitor.RequestRunAsync("Tenant1", "Task1");

        monitor.ResetRunRequested();

        var runRequested = monitor.RunRequested;
        Assert.False(runRequested.IsCompleted);

        await monitor.RequestRunAsync("Tenant1", "Task2");

        Assert.True(runRequested.IsCompleted);
    }

    [Fact]
    public void ResetRunRequested_WithoutRequest_KeepsPendingSignal()
    {
        var monitor = new BackgroundTaskMonitor();
        var runRequested = monitor.RunRequested;

        monitor.ResetRunRequested();

        Assert.Same(runRequested, monitor.RunRequested);
    }

    private static BackgroundTaskScheduler AddScheduler(BackgroundTaskMonitor monitor, string tenant, string name)
    {
        var scheduler = new BackgroundTaskScheduler(tenant, name, _referenceTime, new FakeClock(_referenceTime))
        {
            Settings = new BackgroundTaskSettings { Name = name },
            Updated = true,
        };

        monitor.Schedulers[BackgroundTaskMonitor.GetKey(tenant, name)] = scheduler;

        return scheduler;
    }
}
