using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Events;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.Tests.BackgroundTasks;

public class ModularBackgroundServiceTests
{
    private const string TenantA = "TenantA";
    private const string TenantB = "TenantB";

    private static readonly string _firstTask = typeof(FirstTask).FullName;
    private static readonly string _secondTask = typeof(SecondTask).FullName;

    private static readonly TimeSpan _waitTimeout = TimeSpan.FromSeconds(30);

    [Fact]
    public async Task RequestRun_LongPollingTime_RunsWithoutWaitingForThePolling()
    {
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromHours(1));
        host.AddTenant(TenantA, [new FirstTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        Assert.Equal(BackgroundTaskRunRequestResult.Queued, await host.Monitor.RequestRunAsync(TenantA, _firstTask));
        var state = await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        Assert.Equal(1, work.Count);
        Assert.Equal(BackgroundTaskRunResult.Succeeded, state.LastRun.Result);
        Assert.Equal(BackgroundTaskTrigger.Manual, state.LastRun.Trigger);
    }

    [Fact]
    public async Task RequestRun_DisabledTask_RunsAndKeepsItDisabled()
    {
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromHours(1));
        host.AddTenant(TenantA, [new FirstTask(work)], settings: name => new BackgroundTaskSettings { Name = name, Enable = false });

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        Assert.Equal(1, work.Count);
        Assert.False(host.Monitor.Schedulers[(TenantA, _firstTask)].Settings.Enable);
    }

    [Fact]
    public async Task RequestRun_TwoTenants_OnlyRunsTheTaskOfTheRequestedTenant()
    {
        var workA = new TaskWork();
        var workB = new TaskWork();
        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(workA)]);
        host.AddTenant(TenantB, [new FirstTask(workB)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);
        await host.WaitForTasksAsync(TenantB, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        var stateB = await host.Monitor.GetStateAsync(TenantB, _firstTask);

        Assert.Equal(1, workA.Count);
        Assert.Equal(0, workB.Count);
        Assert.Equal(0, stateB.RunCount);
        Assert.Null(stateB.LastRun);
        Assert.Equal(BackgroundTaskStatus.Idle, stateB.Status);
    }

    [Fact]
    public async Task RequestRun_WhileATaskOfAnotherTenantRunsWithinThePollingTime_StaysQueuedUntilItCompletes()
    {
        var blockingWork = new TaskWork { Blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously) };
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromHours(1));
        host.AddTenant(TenantA, [new FirstTask(blockingWork)]);
        host.AddTenant(TenantB, [new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);
        await host.WaitForTasksAsync(TenantB, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);

        await host.Monitor.RequestRunAsync(TenantB, _secondTask);

        // The background service waits for the tasks of an iteration, up to its polling time, before starting the next one.
        await Task.Delay(TimeSpan.FromSeconds(2), TestContext.Current.CancellationToken);
        Assert.Equal(BackgroundTaskStatus.Queued, (await host.Monitor.GetStateAsync(TenantB, _secondTask)).Status);
        Assert.Equal(0, work.Count);

        blockingWork.Blocked.SetResult();

        await host.WaitForRunCountAsync(TenantB, _secondTask, 1);
        Assert.Equal(1, work.Count);
    }

    [Fact]
    public async Task RequestRun_WhileATaskOfAnotherTenantNeverCompletes_RunsWithoutWaitingForIt()
    {
        var stuckWork = new TaskWork { WaitForCancellation = true };
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(stuckWork)]);
        host.AddTenant(TenantB, [new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);
        await host.WaitForTasksAsync(TenantB, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);

        await host.Monitor.RequestRunAsync(TenantB, _secondTask);
        var state = await host.WaitForRunCountAsync(TenantB, _secondTask, 1);

        Assert.Equal(1, work.Count);
        Assert.Equal(BackgroundTaskTrigger.Manual, state.LastRun.Trigger);
        Assert.Equal(BackgroundTaskStatus.Running, (await host.Monitor.GetStateAsync(TenantA, _firstTask)).Status);
    }

    [Fact]
    public async Task Schedule_TaskThatNeverCompletes_StillRunsTheTasksOfTheOtherTenants()
    {
        var stuckWork = new TaskWork { WaitForCancellation = true };
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(stuckWork)]);
        host.AddTenant(TenantB, [new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);
        await host.WaitForTasksAsync(TenantB, 1);

        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);
        await host.WaitForRunCountAsync(TenantB, _secondTask, 1);

        // TenantB's first run can start alongside TenantA's task, so only its next run proves it isn't held up.
        host.Clock.Advance(TimeSpan.FromMinutes(2));
        var state = await host.WaitForRunCountAsync(TenantB, _secondTask, 2);

        Assert.Equal(2, work.Count);
        Assert.Equal(BackgroundTaskTrigger.Schedule, state.LastRun.Trigger);
        Assert.Equal(1, stuckWork.Count);
    }

    [Fact]
    public async Task Schedule_TaskThatBlocksItsThread_StillRunsTheTasksOfTheOtherTenants()
    {
        // The service starts each run on its own, so that a task that blocks its thread doesn't block the service.
        var threadBlockingWork = new TaskWork { BlocksItsThread = true };
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(threadBlockingWork)]);
        host.AddTenant(TenantB, [new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);
        await host.WaitForTasksAsync(TenantB, 1);

        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);
        await host.WaitForRunCountAsync(TenantB, _secondTask, 1);

        // TenantB's first run can start alongside TenantA's task, so only its next run proves it isn't held up.
        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await host.WaitForRunCountAsync(TenantB, _secondTask, 2);

        Assert.Equal(2, work.Count);
        Assert.Equal(1, threadBlockingWork.Count);
    }

    [Fact]
    public async Task Schedule_TaskThatNeverCompletes_StillRunsTheOtherTasksOfItsTenant()
    {
        var stuckWork = new TaskWork { WaitForCancellation = true };
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(stuckWork), new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 2);

        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);
        await host.WaitForRunCountAsync(TenantA, _secondTask, 1);

        // The second task can run before the first one, so only its next run proves it isn't held up.
        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await host.WaitForRunCountAsync(TenantA, _secondTask, 2);

        Assert.Equal(2, work.Count);
        Assert.Equal(1, stuckWork.Count);
    }

    [Fact]
    public async Task Schedule_TaskStillRunningWhenDueAgain_IsNotRunAgainUntilItCompletes()
    {
        var blockingWork = new TaskWork { Blocked = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously) };
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(blockingWork)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);

        // The service doesn't wait for a run longer than the polling time, so it has to skip the task
        // until that run completes, even when the task is due again.
        host.Clock.Advance(TimeSpan.FromMinutes(2));
        await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);

        Assert.Equal(1, blockingWork.Count);
        Assert.Equal(BackgroundTaskRunRequestResult.AlreadyRunning, await host.Monitor.RequestRunAsync(TenantA, _firstTask));

        // Once it completes, the task runs again for the occurrence it missed.
        blockingWork.Blocked.SetResult();
        var state = await host.WaitForRunCountAsync(TenantA, _firstTask, 2);

        Assert.Equal(2, blockingWork.Count);
        Assert.Equal(BackgroundTaskRunResult.Succeeded, state.LastRun.Result);
    }

    [Fact]
    public async Task Run_TaskLongerThanThePollingTime_LogsASingleWarning()
    {
        var logger = new TestLogger();
        var longWork = new TaskWork { WaitForCancellation = true };
        var work = new TaskWork();
        await using var host = new TestHost(logger: logger);
        host.AddTenant(TenantA, [new FirstTask(longWork), new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 2);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);

        // The second task only runs once the service has stopped waiting for the first one, which is when it logs the warning.
        await host.Monitor.RequestRunAsync(TenantA, _secondTask);
        await host.WaitForRunCountAsync(TenantA, _secondTask, 1);

        // Let the background service poll a few times while the long task is still running.
        await Task.Delay(TimeSpan.FromMilliseconds(500), TestContext.Current.CancellationToken);

        var warning = Assert.Single(logger.Warnings, message => message.Contains(_firstTask, StringComparison.Ordinal));
        Assert.Contains(TenantA, warning);
    }

    [Fact]
    public async Task Schedule_DueTask_RunsOncePerOccurrence()
    {
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        host.Clock.Advance(TimeSpan.FromMinutes(2));
        var state = await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        Assert.Equal(BackgroundTaskTrigger.Schedule, state.LastRun.Trigger);

        await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
        Assert.Equal(1, work.Count);
    }

    [Theory]
    [InlineData("not a cron expression")]
    [InlineData("0 0 31 2 *")]
    [InlineData("")]
    [InlineData(null)]
    public async Task Schedule_UnusableScheduleOfATask_StillRunsTheOtherTasksOfAllTenants(string schedule)
    {
        var unusableWork = new TaskWork();
        var sameTenantWork = new TaskWork();
        var otherTenantWork = new TaskWork();
        using var changes = new CancellationTokenSource();

        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(
            TenantA,
            [new FirstTask(unusableWork), new SecondTask(sameTenantWork)],
            settings: name => name == _firstTask
                ? new BackgroundTaskSettings { Name = name, Schedule = schedule }
                : new BackgroundTaskSettings { Name = name },
            changeToken: new CancellationChangeToken(changes.Token));
        host.AddTenant(TenantB, [new SecondTask(otherTenantWork)]);

        await host.StartAndWaitForTasksAsync(TenantA, 2);
        await host.WaitForTasksAsync(TenantB, 1);

        // The settings are read again, while the scheduler holds the unusable schedule.
        host.ResetChangeToken(TenantA);
        await changes.CancelAsync();

        host.Clock.Advance(TimeSpan.FromMinutes(2));

        await host.WaitForRunCountAsync(TenantA, _secondTask, 1);
        await host.WaitForRunCountAsync(TenantB, _secondTask, 1);

        Assert.Equal(0, unusableWork.Count);
        Assert.Null((await host.Monitor.GetStateAsync(TenantA, _firstTask)).NextStartTime);
    }

    [Fact]
    public async Task Run_FailingTask_RecordsTheFailureAndRunsTheNextTask()
    {
        var failingWork = new TaskWork { Exception = new InvalidOperationException("Failing on purpose.") };
        var work = new TaskWork();
        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(failingWork), new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 2);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.Monitor.RequestRunAsync(TenantA, _secondTask);

        var failedState = await host.WaitForRunCountAsync(TenantA, _firstTask, 1);
        var state = await host.WaitForRunCountAsync(TenantA, _secondTask, 1);

        Assert.Equal(BackgroundTaskRunResult.Failed, failedState.LastRun.Result);
        Assert.Equal("Failing on purpose.", failedState.LastRun.ErrorMessage);
        Assert.Contains(nameof(InvalidOperationException), failedState.LastRun.ErrorDetails);
        Assert.Equal(1, failedState.FailureCount);
        Assert.Equal(BackgroundTaskStatus.Idle, failedState.Status);
        Assert.Equal(BackgroundTaskRunResult.Succeeded, state.LastRun.Result);
    }

    [Fact]
    public async Task Run_TaskCanceledOutsideOfTheShutdown_RecordsAFailedRun()
    {
        var work = new TaskWork { Exception = new OperationCanceledException("Canceled by the task.") };
        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        var state = await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        Assert.Equal(BackgroundTaskRunResult.Failed, state.LastRun.Result);
        Assert.Equal(1, state.FailureCount);
    }

    [Fact]
    public async Task Stop_RunningTask_RecordsACanceledRunThatIsNotAFailure()
    {
        var work = new TaskWork { WaitForCancellation = true };
        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);

        await host.StopAsync();

        var state = await host.Monitor.GetStateAsync(TenantA, _firstTask);

        Assert.Equal(BackgroundTaskRunResult.Canceled, state.LastRun.Result);
        Assert.Equal(0, state.FailureCount);
        Assert.Equal(BackgroundTaskStatus.Idle, state.Status);
    }

    [Fact]
    public async Task Stop_TaskLongerThanThePollingTime_WaitsForItToComplete()
    {
        var stuckWork = new TaskWork { BlockedIgnoringCancellation = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously) };
        var work = new TaskWork();
        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(stuckWork), new SecondTask(work)]);

        try
        {
            await host.StartAndWaitForTasksAsync(TenantA, 2);

            await host.Monitor.RequestRunAsync(TenantA, _firstTask);
            await host.WaitForStatusAsync(TenantA, _firstTask, BackgroundTaskStatus.Running);

            // The second task only runs once the service has stopped waiting for the stuck one.
            await host.Monitor.RequestRunAsync(TenantA, _secondTask);
            await host.WaitForRunCountAsync(TenantA, _secondTask, 1);

            var stop = host.StopAsync();

            await Task.Delay(TimeSpan.FromMilliseconds(200), TestContext.Current.CancellationToken);
            Assert.False(stop.IsCompleted);

            stuckWork.BlockedIgnoringCancellation.SetResult();
            await stop;

            var state = await host.Monitor.GetStateAsync(TenantA, _firstTask);

            Assert.Equal(BackgroundTaskRunResult.Succeeded, state.LastRun.Result);
            Assert.Equal(BackgroundTaskStatus.Idle, state.Status);
        }
        finally
        {
            stuckWork.BlockedIgnoringCancellation.TrySetResult();
        }
    }

    [Fact]
    public async Task RequestRun_AtomicTaskWhoseLockIsHeld_StaysQueuedUntilTheLockIsReleased()
    {
        var work = new TaskWork();
        var distributedLock = new TestDistributedLock { Available = false };

        await using var host = new TestHost(pollingTime: TimeSpan.FromMilliseconds(20));
        host.AddTenant(TenantA, [new FirstTask(work)], settings: CreateAtomicSettings, distributedLock: distributedLock);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await WaitUntilAsync(() => Task.FromResult(distributedLock.AttemptCount >= 2));

        var state = await host.Monitor.GetStateAsync(TenantA, _firstTask);
        Assert.Equal(BackgroundTaskStatus.Queued, state.Status);
        Assert.Equal(0, state.RunCount);
        Assert.Equal(0, work.Count);

        distributedLock.Available = true;
        state = await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        Assert.Equal(1, work.Count);
        Assert.Equal(BackgroundTaskTrigger.Manual, state.LastRun.Trigger);
    }

    [Fact]
    public async Task RequestRun_AtomicTask_HoldsTheLockOfTheTaskDuringTheRun()
    {
        var distributedLock = new TestDistributedLock();
        var work = new TaskWork { OnRun = () => Assert.Equal(1, distributedLock.HeldCount) };

        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(work)], settings: CreateAtomicSettings, distributedLock: distributedLock);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        var state = await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        Assert.Equal(BackgroundTaskRunResult.Succeeded, state.LastRun.Result);
        Assert.Equal($"{_firstTask}_LOCK", distributedLock.LastKey);
        Assert.Equal(TimeSpan.FromMilliseconds(1000), distributedLock.LastTimeout);
        Assert.Equal(TimeSpan.FromMilliseconds(5000), distributedLock.LastExpiration);
        await WaitUntilAsync(() => Task.FromResult(distributedLock.HeldCount == 0));
    }

    [Fact]
    public async Task ReloadTenant_TaskThatRan_KeepsItsExecutionState()
    {
        var work = new TaskWork();
        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 1);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        await host.WaitForRunCountAsync(TenantA, _firstTask, 1);

        await host.ReloadTenantAsync(TenantA, [new FirstTask(work)]);

        await host.Monitor.RequestRunAsync(TenantA, _firstTask);
        var state = await host.WaitForRunCountAsync(TenantA, _firstTask, 2);

        Assert.Equal(2, work.Count);
        Assert.Equal(2, state.RecentRuns.Count);
    }

    [Fact]
    public async Task ReloadTenant_TaskNoLongerRegistered_RemovesItsExecutionState()
    {
        var work = new TaskWork();
        await using var host = new TestHost();
        host.AddTenant(TenantA, [new FirstTask(work), new SecondTask(work)]);

        await host.StartAndWaitForTasksAsync(TenantA, 2);

        await host.ReloadTenantAsync(TenantA, [new SecondTask(work)]);

        await host.WaitForTasksAsync(TenantA, 1);

        Assert.Null(await host.Monitor.GetStateAsync(TenantA, _firstTask));
        Assert.Equal(BackgroundTaskRunRequestResult.NotFound, await host.Monitor.RequestRunAsync(TenantA, _firstTask));
    }

    private static async Task WaitUntilAsync(Func<Task<bool>> condition)
    {
        var timeout = DateTime.UtcNow + _waitTimeout;
        while (!await condition())
        {
            Assert.True(DateTime.UtcNow < timeout, "The condition was not met in time.");

            await Task.Delay(TimeSpan.FromMilliseconds(10), TestContext.Current.CancellationToken);
        }
    }

    private static BackgroundTaskSettings CreateAtomicSettings(string name)
        => new() { Name = name, LockTimeout = 1000, LockExpiration = 5000 };

    private sealed class TaskWork
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);
        public Exception Exception { get; init; }
        public TaskCompletionSource Blocked { get; init; }
        public TaskCompletionSource BlockedIgnoringCancellation { get; init; }
        public bool BlocksItsThread { get; init; }
        public bool WaitForCancellation { get; init; }
        public Action OnRun { get; init; }

        public async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _count);
            OnRun?.Invoke();

            // Waits synchronously before the first await, so it blocks its thread until the service stops.
            if (BlocksItsThread)
            {
                cancellationToken.WaitHandle.WaitOne();
            }

            if (Blocked is not null)
            {
                await Blocked.Task.WaitAsync(cancellationToken);
            }

            // Stopping the service has to wait until the test releases it.
            if (BlockedIgnoringCancellation is not null)
            {
                await BlockedIgnoringCancellation.Task;
            }

            if (WaitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            if (Exception is not null)
            {
                throw Exception;
            }
        }
    }

    [BackgroundTask(Schedule = "* * * * *")]
    private sealed class FirstTask : IBackgroundTask
    {
        private readonly TaskWork _work;

        public FirstTask(TaskWork work)
        {
            _work = work;
        }

        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            => _work.DoWorkAsync(cancellationToken);
    }

    [BackgroundTask(Schedule = "* * * * *")]
    private sealed class SecondTask : IBackgroundTask
    {
        private readonly TaskWork _work;

        public SecondTask(TaskWork work)
        {
            _work = work;
        }

        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
            => _work.DoWorkAsync(cancellationToken);
    }

    private sealed class TestSettingsProvider : IBackgroundTaskSettingsProvider
    {
        private readonly Func<string, BackgroundTaskSettings> _settings;

        public TestSettingsProvider(Func<string, BackgroundTaskSettings> settings, IChangeToken changeToken)
        {
            _settings = settings;
            ChangeToken = changeToken;
        }

        public IChangeToken ChangeToken { get; set; }

        public Task<BackgroundTaskSettings> GetSettingsAsync(IBackgroundTask task)
            => Task.FromResult(_settings(task.GetTaskName()));
    }

    private sealed class TestDistributedLock : IDistributedLock
    {
        private int _attemptCount;
        private int _heldCount;
        private volatile bool _available = true;

        public bool Available { get => _available; set => _available = value; }
        public int AttemptCount => Volatile.Read(ref _attemptCount);
        public int HeldCount => Volatile.Read(ref _heldCount);
        public string LastKey { get; private set; }
        public TimeSpan LastTimeout { get; private set; }
        public TimeSpan? LastExpiration { get; private set; }

        public Task<ILocker> AcquireLockAsync(string key, TimeSpan? expiration = null)
            => throw new NotSupportedException();

        public Task<bool> IsLockAcquiredAsync(string key)
            => Task.FromResult(HeldCount > 0);

        public Task<(ILocker locker, bool locked)> TryAcquireLockAsync(string key, TimeSpan timeout, TimeSpan? expiration = null)
        {
            Interlocked.Increment(ref _attemptCount);

            LastKey = key;
            LastTimeout = timeout;
            LastExpiration = expiration;

            if (!Available)
            {
                return Task.FromResult<(ILocker, bool)>((null, false));
            }

            Interlocked.Increment(ref _heldCount);

            return Task.FromResult<(ILocker, bool)>((new Locker(this), true));
        }

        private sealed class Locker : ILocker
        {
            private readonly TestDistributedLock _owner;

            public Locker(TestDistributedLock owner)
            {
                _owner = owner;
            }

            public void Dispose() => Interlocked.Decrement(ref _owner._heldCount);

            public ValueTask DisposeAsync()
            {
                Dispose();

                return ValueTask.CompletedTask;
            }
        }
    }

    private sealed class TestLogger : ILogger<ModularBackgroundService>
    {
        private readonly ConcurrentQueue<(LogLevel Level, string Message)> _entries = new();

        public string[] Warnings => _entries
            .Where(entry => entry.Level == LogLevel.Warning)
            .Select(entry => entry.Message)
            .ToArray();

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => _entries.Enqueue((logLevel, formatter(state, exception)));
    }

    private sealed class TestShellHost : IShellHost
    {
        private readonly Dictionary<string, ShellContext> _shells = [];
        private readonly Lock _lock = new();

        public ShellsEvent LoadingAsync { get; set; }
        public ShellEvent ReleasingAsync { get; set; }
        public ShellEvent ReloadingAsync { get; set; }
        public ShellEvent RemovingAsync { get; set; }

        public void SetShell(ShellContext shell)
        {
            lock (_lock)
            {
                _shells[shell.Settings.Name] = shell;
            }
        }

        public IEnumerable<ShellContext> ListShellContexts()
        {
            lock (_lock)
            {
                return _shells.Values.ToArray();
            }
        }

        public bool TryGetShellContext(string name, out ShellContext shellContext)
        {
            lock (_lock)
            {
                return _shells.TryGetValue(name, out shellContext);
            }
        }

        public bool TryGetSettings(string name, out ShellSettings settings)
        {
            var found = TryGetShellContext(name, out var shell);
            settings = shell?.Settings;

            return found;
        }

        public async Task<ShellScope> GetScopeAsync(ShellSettings settings)
        {
            TryGetShellContext(settings.Name, out var shell);

            return await shell.CreateScopeAsync();
        }

        public IEnumerable<ShellSettings> GetAllSettings() => ListShellContexts().Select(shell => shell.Settings);

        public Task InitializeAsync() => Task.CompletedTask;

        public Task<ShellContext> GetOrCreateShellContextAsync(ShellSettings settings)
            => throw new NotSupportedException();

        public Task UpdateShellSettingsAsync(ShellSettings settings)
            => throw new NotSupportedException();

        public Task ReloadShellContextAsync(ShellSettings settings, bool eventSource = true)
            => throw new NotSupportedException();

        public Task ReleaseShellContextAsync(ShellSettings settings, bool eventSource = true)
            => throw new NotSupportedException();

        public Task RemoveShellSettingsAsync(ShellSettings settings)
            => throw new NotSupportedException();

        public Task RemoveShellContextAsync(ShellSettings settings, bool eventSource = true)
            => throw new NotSupportedException();

        public Task ChangedAsync(ShellDescriptor descriptor, ShellSettings settings)
            => throw new NotSupportedException();
    }

    private sealed class TestHost : IAsyncDisposable
    {
        private readonly TestShellHost _shellHost = new();
        private readonly Dictionary<string, TestSettingsProvider> _settingsProviders = [];
        private readonly List<ServiceProvider> _serviceProviders = [];
        private readonly ModularBackgroundService _service;
        private bool _started;

        public TestHost(TimeSpan? pollingTime = null, ILogger<ModularBackgroundService> logger = null)
        {
            _service = new ModularBackgroundService(
                _shellHost,
                Monitor,
                new HttpContextAccessor(),
                Options.Create(new BackgroundServiceOptions
                {
                    ShellWarmup = true,
                    PollingTime = pollingTime ?? TimeSpan.FromMilliseconds(100),
                    MinimumIdleTime = TimeSpan.FromMilliseconds(10),
                }),
                logger ?? NullLogger<ModularBackgroundService>.Instance,
                Clock);
        }

        public BackgroundTaskMonitor Monitor { get; } = new();

        public FakeClock Clock { get; } = new(DateTime.UtcNow);

        public void AddTenant(
            string tenant,
            IBackgroundTask[] tasks,
            Func<string, BackgroundTaskSettings> settings = null,
            IChangeToken changeToken = null,
            IDistributedLock distributedLock = null)
        {
            var services = new ServiceCollection();

            foreach (var task in tasks)
            {
                services.AddSingleton(task);
            }

            services.AddSingleton(distributedLock ?? new TestDistributedLock());

            var settingsProvider = new TestSettingsProvider(
                settings ?? (name => new BackgroundTaskSettings { Name = name }),
                changeToken ?? NullChangeToken.Singleton);

            _settingsProviders[tenant] = settingsProvider;
            services.AddSingleton<IBackgroundTaskSettingsProvider>(settingsProvider);

            var serviceProvider = services.BuildServiceProvider();
            _serviceProviders.Add(serviceProvider);

            _shellHost.SetShell(new ShellContext
            {
                Settings = new ShellSettings { Name = tenant }.AsRunning(),
                ServiceProvider = serviceProvider,
                IsActivated = true,
            });
        }

        // Lets the next change of the settings be followed by a token that doesn't change anymore.
        public void ResetChangeToken(string tenant)
            => _settingsProviders[tenant].ChangeToken = NullChangeToken.Singleton;

        public async Task ReloadTenantAsync(string tenant, IBackgroundTask[] tasks)
        {
            _shellHost.TryGetShellContext(tenant, out var previous);

            // A new shell has its own creation time, which is how the background service detects a reload.
            await Task.Delay(TimeSpan.FromMilliseconds(5), TestContext.Current.CancellationToken);

            AddTenant(tenant, tasks);
            await previous.ReleaseAsync();
        }

        public async Task StartAndWaitForTasksAsync(string tenant, int count)
        {
            _started = true;
            await _service.StartAsync(TestContext.Current.CancellationToken);
            await WaitForTasksAsync(tenant, count);
        }

        public Task WaitForTasksAsync(string tenant, int count) => WaitUntilAsync(async () =>
        {
            var states = await Monitor.GetStatesAsync(tenant);

            return states.Count == count && states.All(state => Monitor.Schedulers[(tenant, state.Name)].Updated);
        });

        public Task WaitForStatusAsync(string tenant, string name, BackgroundTaskStatus status) => WaitUntilAsync(async ()
            => (await Monitor.GetStateAsync(tenant, name))?.Status == status);

        public async Task<BackgroundTaskState> WaitForRunCountAsync(string tenant, string name, long runCount)
        {
            await WaitUntilAsync(async () =>
            {
                var state = await Monitor.GetStateAsync(tenant, name);

                return state?.RunCount >= runCount && state.Status == BackgroundTaskStatus.Idle;
            });

            return await Monitor.GetStateAsync(tenant, name);
        }

        public async Task StopAsync()
        {
            // Stops waiting after a timeout, as a host does on shutdown, so a run that never completes doesn't hang the test.
            using var timeout = new CancellationTokenSource(_waitTimeout);
            await _service.StopAsync(timeout.Token);
        }

        public async ValueTask DisposeAsync()
        {
            if (_started)
            {
                await StopAsync();
            }

            _service.Dispose();

            foreach (var serviceProvider in _serviceProviders)
            {
                await serviceProvider.DisposeAsync();
            }
        }
    }
}
