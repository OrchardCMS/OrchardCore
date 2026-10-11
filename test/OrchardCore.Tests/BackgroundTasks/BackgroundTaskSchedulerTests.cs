using OrchardCore.BackgroundTasks;

namespace OrchardCore.Tests.BackgroundTasks;

public class BackgroundTaskSchedulerTests
{
    private const string Tenant = "Default";
    private const string TaskName = "Tests.TestTask";

    private static readonly DateTime _referenceTime = new(2026, 1, 15, 10, 0, 30, DateTimeKind.Utc);

    [Fact]
    public void CanRun_DueEnabledTask_ReturnsTrue()
    {
        var clock = new FakeClock(_referenceTime.AddSeconds(30));
        var scheduler = CreateScheduler(clock, "* * * * *");

        Assert.True(scheduler.CanRun());
    }

    [Fact]
    public void CanRun_NotDueTask_ReturnsFalse()
    {
        var clock = new FakeClock(_referenceTime.AddSeconds(10));
        var scheduler = CreateScheduler(clock, "* * * * *");

        Assert.False(scheduler.CanRun());
    }

    [Fact]
    public void CanRun_DueDisabledTask_ReturnsFalse()
    {
        var clock = new FakeClock(_referenceTime.AddMinutes(5));
        var scheduler = CreateScheduler(clock, "* * * * *", enable: false);

        Assert.False(scheduler.CanRun());
    }

    [Fact]
    public void CanRun_QueuedDisabledTaskNotDue_ReturnsTrue()
    {
        var clock = new FakeClock(_referenceTime.AddSeconds(1));
        var scheduler = CreateScheduler(clock, "0 0 * * *", enable: false);

        scheduler.RequestRun();

        Assert.True(scheduler.CanRun());
    }

    [Fact]
    public void CanRun_QueuedReleasedTask_ReturnsFalse()
    {
        var clock = new FakeClock(_referenceTime.AddSeconds(1));
        var scheduler = CreateScheduler(clock, "0 0 * * *");

        scheduler.RequestRun();
        scheduler.Released = true;

        Assert.False(scheduler.CanRun());
    }

    [Fact]
    public void RequestRun_IdleTask_QueuesIt()
    {
        var clock = new FakeClock(_referenceTime.AddSeconds(1));
        var scheduler = CreateScheduler(clock, "0 0 * * *");

        var result = scheduler.RequestRun();

        Assert.Equal(BackgroundTaskRunRequestResult.Queued, result);

        var state = scheduler.GetState();
        Assert.Equal(BackgroundTaskStatus.Queued, state.Status);
        Assert.Equal(clock.UtcNow, state.QueuedTime);
    }

    [Fact]
    public void RequestRun_QueuedTask_ReturnsAlreadyQueued()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "0 0 * * *");
        scheduler.RequestRun();

        var result = scheduler.RequestRun();

        Assert.Equal(BackgroundTaskRunRequestResult.AlreadyQueued, result);
    }

    [Fact]
    public void RequestRun_RunningTask_ReturnsAlreadyRunning()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "0 0 * * *");
        scheduler.Run();

        var result = scheduler.RequestRun();

        Assert.Equal(BackgroundTaskRunRequestResult.AlreadyRunning, result);
        Assert.Equal(BackgroundTaskStatus.Running, scheduler.GetState().Status);
    }

    [Fact]
    public void Run_QueuedTask_MarksRunningAndClearsQueue()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "0 0 * * *");
        scheduler.RequestRun();

        clock.Advance(TimeSpan.FromSeconds(5));
        scheduler.Run();

        var state = scheduler.GetState();
        Assert.Equal(BackgroundTaskStatus.Running, state.Status);
        Assert.Null(state.QueuedTime);
        Assert.Equal(clock.UtcNow, state.LastStartTime);
        Assert.Equal(clock.UtcNow, scheduler.ReferenceTime);
        Assert.False(scheduler.CanRun());
    }

    [Theory]
    [InlineData(true, BackgroundTaskTrigger.Manual)]
    [InlineData(false, BackgroundTaskTrigger.Schedule)]
    public void Complete_AfterRun_RecordsTrigger(bool requested, BackgroundTaskTrigger expectedTrigger)
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *");

        if (requested)
        {
            scheduler.RequestRun();
        }

        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Succeeded);

        Assert.Equal(expectedTrigger, scheduler.GetState().LastRun.Trigger);
    }

    [Fact]
    public void Complete_SucceededRun_RecordsRun()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *");
        var startTime = clock.UtcNow;

        scheduler.Run();
        clock.Advance(TimeSpan.FromMilliseconds(1500));
        scheduler.Complete(BackgroundTaskRunResult.Succeeded);

        var state = scheduler.GetState();
        Assert.Equal(BackgroundTaskStatus.Idle, state.Status);
        Assert.Equal(1, state.RunCount);
        Assert.Equal(0, state.FailureCount);
        Assert.NotNull(state.LastRun);
        Assert.Equal(BackgroundTaskRunResult.Succeeded, state.LastRun.Result);
        Assert.Equal(startTime, state.LastRun.StartTime);
        Assert.Equal(clock.UtcNow, state.LastRun.EndTime);
        Assert.Equal(TimeSpan.FromMilliseconds(1500), state.LastRun.Duration);
        Assert.Null(state.LastRun.ErrorMessage);
        Assert.Null(state.LastRun.ErrorDetails);
        Assert.Same(state.LastRun, Assert.Single(state.RecentRuns));
    }

    [Fact]
    public void Complete_FailedRun_RecordsError()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *");
        var exception = CreateThrownException("The task failed.");

        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Failed, exception);

        var state = scheduler.GetState();
        Assert.Equal(BackgroundTaskStatus.Idle, state.Status);
        Assert.Equal(1, state.RunCount);
        Assert.Equal(1, state.FailureCount);
        Assert.Equal(BackgroundTaskRunResult.Failed, state.LastRun.Result);
        Assert.Equal("The task failed.", state.LastRun.ErrorMessage);
        Assert.Contains(nameof(InvalidOperationException), state.LastRun.ErrorDetails);
        Assert.Contains(nameof(CreateThrownException), state.LastRun.ErrorDetails);
    }

    [Fact]
    public void Complete_CanceledRun_IsNotCountedAsFailure()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *");

        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Canceled, new OperationCanceledException());

        var state = scheduler.GetState();
        Assert.Equal(BackgroundTaskRunResult.Canceled, state.LastRun.Result);
        Assert.Equal(1, state.RunCount);
        Assert.Equal(0, state.FailureCount);
    }

    [Fact]
    public void Complete_MoreRunsThanLimit_KeepsMostRecentRunsFirst()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *", maxRecentRuns: 3);
        var startTimes = new List<DateTime>();

        for (var i = 0; i < 5; i++)
        {
            clock.Advance(TimeSpan.FromMinutes(1));
            startTimes.Add(clock.UtcNow);

            scheduler.Run();
            scheduler.Complete(i % 2 == 0 ? BackgroundTaskRunResult.Succeeded : BackgroundTaskRunResult.Failed);
        }

        var state = scheduler.GetState();
        Assert.Equal(5, state.RunCount);
        Assert.Equal(2, state.FailureCount);
        Assert.Equal([startTimes[4], startTimes[3], startTimes[2]], state.RecentRuns.Select(run => run.StartTime));
        Assert.Same(state.RecentRuns[0], state.LastRun);
    }

    [Fact]
    public void Complete_NoRecentRunsKept_StillRecordsLastRun()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *", maxRecentRuns: 0);

        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Succeeded);

        var state = scheduler.GetState();
        Assert.Empty(state.RecentRuns);
        Assert.NotNull(state.LastRun);
    }

    [Fact]
    public void GetState_NextRuns_DoNotUpdatePreviousSnapshot()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *");

        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Succeeded);
        var snapshot = scheduler.GetState();

        scheduler.Run();
        scheduler.Complete(BackgroundTaskRunResult.Failed, new InvalidOperationException());

        Assert.Equal(1, snapshot.RunCount);
        Assert.Equal(BackgroundTaskRunResult.Succeeded, snapshot.LastRun.Result);
        Assert.Single(snapshot.RecentRuns);
        Assert.Equal(BackgroundTaskStatus.Idle, snapshot.Status);
    }

    [Fact]
    public void GetState_NewScheduler_HasNeverRun()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *");

        var state = scheduler.GetState();

        Assert.Equal(Tenant, state.Tenant);
        Assert.Equal(TaskName, state.Name);
        Assert.Equal(BackgroundTaskStatus.Idle, state.Status);
        Assert.Null(state.LastRun);
        Assert.Equal(DateTime.MinValue, state.LastStartTime);
        Assert.Equal(0, state.RunCount);
        Assert.Empty(state.RecentRuns);
    }

    [Fact]
    public void GetNextStartTime_WithoutTimeZone_ReturnsNextOccurrenceAfterReferenceTime()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "15 * * * *");

        var nextStartTime = scheduler.GetNextStartTime();

        Assert.Equal(new DateTime(2026, 1, 15, 10, 15, 0, DateTimeKind.Utc), nextStartTime);
        Assert.Equal(DateTimeKind.Utc, nextStartTime.Value.Kind);
    }

    [Theory]
    [InlineData(1, 8)] // Eastern Standard Time, UTC-5.
    [InlineData(7, 7)] // Eastern Daylight Time, UTC-4.
    public void GetNextStartTime_WithTimeZone_AppliesScheduleToLocalTime(int month, int expectedUtcHour)
    {
        var referenceTime = new DateTime(2026, month, 15, 12, 0, 0, DateTimeKind.Utc);
        var clock = new FakeClock(referenceTime);
        var scheduler = CreateScheduler(clock, "0 3 * * *", referenceTime: referenceTime);
        scheduler.TimeZone = clock.GetTimeZone("America/New_York");

        var nextStartTime = scheduler.GetNextStartTime();

        // At 12:00 UTC it is already past 3:00 in New York, so the next occurrence is on the next day.
        Assert.Equal(new DateTime(2026, month, 16, expectedUtcHour, 0, 0, DateTimeKind.Utc), nextStartTime);
    }

    [Fact]
    public void GetNextStartTime_AfterRun_IsComputedFromRunStart()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "*/5 * * * *");

        clock.UtcNow = new DateTime(2026, 1, 15, 10, 7, 0, DateTimeKind.Utc);
        scheduler.Run();

        Assert.Equal(new DateTime(2026, 1, 15, 10, 10, 0, DateTimeKind.Utc), scheduler.GetNextStartTime());
    }

    [Fact]
    public void GetNextStartTime_DisabledTask_ReturnsNull()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "* * * * *", enable: false);

        Assert.Null(scheduler.GetNextStartTime());
        Assert.Null(scheduler.GetState().NextStartTime);
    }

    [Fact]
    public void GetNextStartTime_InvalidSchedule_ReturnsNull()
    {
        var clock = new FakeClock(_referenceTime);
        var scheduler = CreateScheduler(clock, "not a cron expression");

        Assert.Null(scheduler.GetNextStartTime());
    }

    private static BackgroundTaskScheduler CreateScheduler(
        FakeClock clock,
        string schedule,
        bool enable = true,
        int maxRecentRuns = BackgroundTaskScheduler.DefaultMaxRecentRuns,
        DateTime? referenceTime = null)
    {
        return new BackgroundTaskScheduler(Tenant, TaskName, referenceTime ?? _referenceTime, clock, maxRecentRuns)
        {
            Settings = new BackgroundTaskSettings
            {
                Name = TaskName,
                Schedule = schedule,
                Enable = enable,
            },
            Updated = true,
        };
    }

    private static InvalidOperationException CreateThrownException(string message)
    {
        try
        {
            throw new InvalidOperationException(message);
        }
        catch (InvalidOperationException ex)
        {
            return ex;
        }
    }
}
