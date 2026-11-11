using System.Diagnostics;
using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell;
using OrchardCore.Jobs;
using OrchardCore.Jobs.BackgroundTasks;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Modules;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.DataPipelines;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Jobs;

public sealed class JobsTests
{
    [Fact]
    public async Task Enqueue_RegisteredJob_RunsOnceAndRecordsTheRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();

        // Act
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key }, request => request.Title = "A title");
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal(1, TestJob.GetState(key).Executions);
        Assert.Equal(TestJob.Name, run.JobName);
        Assert.Equal("A title", run.Title);
        Assert.Equal(JobTriggers.Code, run.Trigger);
        Assert.Equal(1, run.Attempt);
        Assert.Equal(100, run.Progress);
        Assert.NotNull(run.StartedUtc);
        Assert.NotNull(run.CompletedUtc);
        Assert.NotNull(run.DurationMilliseconds);
        Assert.Equal(TimeSpan.FromMilliseconds(run.DurationMilliseconds.Value), run.GetDuration());
        Assert.False(string.IsNullOrEmpty(run.NodeId));
        Assert.Contains(run.Log, entry => entry.Message == "Execution 1");
        Assert.Equal(1, run.Result?["Executions"]?.GetValue<int>());
    }

    [Fact]
    public async Task Enqueue_UnknownJob_Throws()
    {
        // Arrange
        using var context = await CreateContextAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IJobManager>();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => manager.EnqueueAsync("NotAJob"));
        });
    }

    [Fact]
    public async Task Enqueue_FailingJob_FailsWithItsError()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();

        // Act
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key, FailCount = 1 });
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Failed, run.Status);
        Assert.Equal("Test failure 1", run.Error);
        Assert.Contains(nameof(InvalidOperationException), run.ErrorDetails, StringComparison.Ordinal);
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task Enqueue_FailingJobWithAttemptsLeft_RunsAgainAndSucceeds()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();

        // Act
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key, FailCount = 2 }, request => request.MaxAttempts = 3);
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal(3, run.Attempt);
        Assert.Equal(3, TestJob.GetState(key).Executions);
        Assert.Null(run.Error);
        Assert.Contains(run.Log, entry => entry.Message.StartsWith("Attempt 1 of 3 failed", StringComparison.Ordinal));
        Assert.Contains(run.Log, entry => entry.Message.StartsWith("Attempt 2 of 3 failed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Enqueue_SameDeduplicationKey_ReturnsTheActiveRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var due = DateTime.UtcNow.AddHours(1);

        // Act
        var first = await EnqueueAsync(context, new TestJobParameters { Key = key }, request =>
        {
            request.DeduplicationKey = key;
            request.DueUtc = due;
        });
        var second = await EnqueueAsync(context, new TestJobParameters { Key = key }, request =>
        {
            request.DeduplicationKey = key;
            request.DueUtc = due;
        });

        // Assert
        Assert.Equal(first, second);
        Assert.Equal(1, await CountRunsAsync(context, index => index.DeduplicationKey == key));
    }

    [Fact]
    public async Task Enqueue_ReplaceQueued_ReschedulesTheRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();

        var first = await EnqueueAsync(context, new TestJobParameters { Key = key, FailCount = 1 }, request =>
        {
            request.DeduplicationKey = key;
            request.DueUtc = DateTime.UtcNow.AddHours(1);
        });

        // Act
        var second = await EnqueueAsync(context, new TestJobParameters { Key = key }, request =>
        {
            request.DeduplicationKey = key;
            request.ReplaceQueued = true;
        });
        var run = await WaitForRunAsync(context, second);

        // Assert
        Assert.Equal(first, second);
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task Enqueue_DueLater_StartsWhenDue()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var due = DateTime.UtcNow.AddSeconds(3);

        // Act
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key }, request => request.DueUtc = due);
        var queued = await GetRunAsync(context, runId);
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Queued, queued.Status);
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.True(run.StartedUtc >= due.AddSeconds(-1.5), $"Started {run.StartedUtc:O}, due {due:O}.");
    }

    [Fact]
    public async Task Cancel_ScheduledRun_CancelsItAtOnce()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key }, request => request.DueUtc = DateTime.UtcNow.AddHours(1));

        // Act
        var canceled = false;
        await context.UsingTenantScopeAsync(async scope => canceled = await scope.ServiceProvider.GetRequiredService<IJobManager>().CancelAsync(runId, "a test"));
        var run = await GetRunAsync(context, runId);

        // Assert
        Assert.True(canceled);
        Assert.Equal(JobRunStatus.Canceled, run.Status);
        Assert.NotNull(run.CompletedUtc);
        Assert.Equal(0, TestJob.GetState(key).Executions);
        Assert.Contains(run.Log, entry => entry.Message.Contains("a test", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Cancel_RunningRun_StopsIt()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key, WaitForCancellation = true });
        await WaitForRunAsync(context, runId, run => run.Status == JobRunStatus.Running && TestJob.GetState(key).Executions == 1);

        // Act
        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IJobManager>().CancelAsync(runId));
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Canceled, run.Status);
        Assert.Equal(1, TestJob.GetState(key).Canceled);
    }

    [Fact]
    public async Task Execute_SeveralServersAtOnce_RunsOnce()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await CreateRunAsync(context, new TestJobParameters { Key = key, DelayMilliseconds = 200 });

        // Act: each scope stands for a server that found the queued run.
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<JobRunExecutor>().ExecuteAsync(runId)))));
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal(1, run.Attempt);
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task Execute_SameConcurrencyKey_RunsOneAtATime()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runIds = new List<string>();

        for (var i = 0; i < 4; i++)
        {
            runIds.Add(await CreateRunAsync(context, new TestJobParameters { Key = key, DelayMilliseconds = 300 }, run => run.ConcurrencyKey = key));
        }

        // Act: several servers each try to execute every run at the same time.
        await Task.WhenAll(runIds.SelectMany(runId => Enumerable.Range(0, 2).Select(_ => Task.Run(() =>
            context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<JobRunExecutor>().ExecuteAsync(runId))))));

        foreach (var runId in runIds)
        {
            // The runs that waited are started as the others end.
            var run = await WaitForRunAsync(context, runId, dispatch: true);
            Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        }

        // Assert
        var state = TestJob.GetState(key);
        Assert.Equal(4, state.Executions);
        Assert.Equal(1, state.MaxConcurrent);
    }

    [Fact]
    public async Task Enqueue_JobWithChildren_WaitsForThemAndSucceeds()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();

        // Act
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key, Children = 3 });
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);

        IReadOnlyList<JobRun> children = [];
        await context.UsingTenantScopeAsync(async scope => children = await scope.ServiceProvider.GetRequiredService<IJobManager>().ListChildrenAsync(runId));

        Assert.Equal(3, children.Count);
        Assert.All(children, child =>
        {
            Assert.Equal(JobRunStatus.Succeeded, child.Status);
            Assert.Equal(JobTriggers.Parent, child.Trigger);
            Assert.True(child.CompletedUtc <= run.CompletedUtc);
        });
    }

    [Fact]
    public async Task Enqueue_JobWithFailingChild_Fails()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();

        // Act
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key, Children = 3, FailingChildren = 1 });
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Failed, run.Status);
        Assert.Equal("1 of its 3 child runs didn't succeed.", run.Error);
    }

    [Fact]
    public async Task QueueDue_SeveralServersAtOnce_QueuesOneRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        string scheduleId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IJobScheduleManager>();
            var schedule = new JobSchedule
            {
                Name = "Every day",
                JobName = TestJob.Name,
                Cron = "0 3 * * *",
                Parameters = JObject.FromObject(new TestJobParameters { Key = key }),
            };

            await manager.SaveAsync(schedule);
            scheduleId = schedule.ScheduleId;

            Assert.True(schedule.NextOccurrenceUtc > DateTime.UtcNow);

            // The schedule is due.
            schedule.NextOccurrenceUtc = DateTime.UtcNow.AddMinutes(-5);
            await scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(schedule, collection: JobRun.Collection);
        });

        // Act: each scope stands for a server whose background task finds the due schedule.
        await Task.WhenAll(Enumerable.Range(0, 6).Select(_ => Task.Run(() =>
            context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<JobScheduleManager>().QueueDueAsync()))));

        // Assert
        Assert.Equal(1, await CountRunsAsync(context, index => index.ScheduleId == scheduleId));

        JobSchedule saved = null;
        await context.UsingTenantScopeAsync(async scope => saved = await scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().GetAsync(scheduleId));

        Assert.True(saved.NextOccurrenceUtc > DateTime.UtcNow);
        Assert.NotNull(saved.LastRunId);

        var run = await WaitForRunAsync(context, saved.LastRunId);
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal(JobTriggers.Schedule, run.Trigger);
        Assert.Equal("Every day", run.Title);
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task List_JobDeclaringASchedule_CreatesItOnce()
    {
        // Arrange
        using var context = await CreateContextAsync();
        IReadOnlyList<JobSchedule> first = [];
        IReadOnlyList<JobSchedule> second = [];

        // Act
        await context.UsingTenantScopeAsync(async scope => first = await scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().ListAsync());
        await context.UsingTenantScopeAsync(async scope => second = await scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().ListAsync());

        // Assert
        var declared = Assert.Single(second, schedule => schedule.JobName == TestJob.DeclaredScheduleName);
        Assert.Single(first, schedule => schedule.JobName == TestJob.DeclaredScheduleName);
        Assert.True(declared.IsDeclared);
        Assert.True(declared.IsEnabled);
        Assert.Equal(TestJob.DeclaredScheduleName, declared.ScheduleId);
        Assert.Equal("0 0 1 1 *", declared.Cron);
        Assert.NotNull(declared.NextOccurrenceUtc);

        // A declared schedule can't be deleted.
        var deleted = true;
        await context.UsingTenantScopeAsync(async scope => deleted = await scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().DeleteAsync(declared.ScheduleId));
        Assert.False(deleted);
    }

    [Fact]
    public async Task BackgroundTask_AbandonedRunWithoutAttemptsLeft_Fails()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await CreateRunAsync(context, new TestJobParameters { Key = key }, run =>
        {
            run.Status = JobRunStatus.Running;
            run.NodeId = "another-server";
            run.Attempt = 1;
            run.StartedUtc = DateTime.UtcNow.AddHours(-2);
            run.HeartbeatUtc = DateTime.UtcNow.AddHours(-1);
        });

        // Act
        await context.UsingTenantScopeAsync(scope => new JobsBackgroundTask().DoWorkAsync(scope.ServiceProvider, CancellationToken.None));
        var run = await GetRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Failed, run.Status);
        Assert.Equal("The server that was executing the run stopped.", run.Error);
        Assert.Equal(0, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task BackgroundTask_AbandonedRunWithAttemptsLeft_RunsAgain()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await CreateRunAsync(context, new TestJobParameters { Key = key }, run =>
        {
            run.Status = JobRunStatus.Running;
            run.NodeId = "another-server";
            run.Attempt = 1;
            run.MaxAttempts = 2;
            run.StartedUtc = DateTime.UtcNow.AddHours(-2);
            run.HeartbeatUtc = DateTime.UtcNow.AddHours(-1);
        });

        // Act
        await context.UsingTenantScopeAsync(scope => new JobsBackgroundTask().DoWorkAsync(scope.ServiceProvider, CancellationToken.None));
        var run = await WaitForRunAsync(context, runId);

        // Assert
        Assert.True(run.Status == JobRunStatus.Succeeded, run.Error);
        Assert.Equal(2, run.Attempt);
        Assert.NotEqual("another-server", run.NodeId);
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task BackgroundTask_RunOfAnotherServerThatReports_IsLeftAlone()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await CreateRunAsync(context, new TestJobParameters { Key = key }, run =>
        {
            run.Status = JobRunStatus.Running;
            run.NodeId = "another-server";
            run.Attempt = 1;
            run.StartedUtc = DateTime.UtcNow.AddHours(-2);
            run.HeartbeatUtc = DateTime.UtcNow.AddSeconds(-5);
        });

        // Act
        await context.UsingTenantScopeAsync(scope => new JobsBackgroundTask().DoWorkAsync(scope.ServiceProvider, CancellationToken.None));
        var run = await GetRunAsync(context, runId);

        // Assert
        Assert.Equal(JobRunStatus.Running, run.Status);
        Assert.Equal("another-server", run.NodeId);
    }

    [Fact]
    public async Task Enqueue_JobNotKeepingSucceededRuns_DeletesTheRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        string runId = null;

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync(TestJob.TransientName, new TestJobParameters { Key = key })).RunId;
        });

        var stopwatch = Stopwatch.StartNew();
        JobRun run;

        while ((run = await GetRunAsync(context, runId)) is not null && stopwatch.Elapsed < TimeSpan.FromSeconds(60))
        {
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }

        // Assert
        Assert.Null(run);
        Assert.Equal(1, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task Rerun_CompletedRun_QueuesANewRun()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var key = NewKey();
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = key }, request => request.Title = "Again");
        await WaitForRunAsync(context, runId);

        // Act
        string rerunId = null;
        await context.UsingTenantScopeAsync(async scope => rerunId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().RerunAsync(runId, "someone"))?.RunId);
        var rerun = await WaitForRunAsync(context, rerunId);

        // Assert
        Assert.NotEqual(runId, rerunId);
        Assert.True(rerun.Status == JobRunStatus.Succeeded, rerun.Error);
        Assert.Equal("Again", rerun.Title);
        Assert.Equal(JobTriggers.Manual, rerun.Trigger);
        Assert.Equal("someone", rerun.TriggeredBy);
        Assert.Equal(2, TestJob.GetState(key).Executions);
    }

    [Fact]
    public async Task Delete_RunThatDidNotEnd_IsRefused()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var runId = await EnqueueAsync(context, new TestJobParameters { Key = NewKey() }, request => request.DueUtc = DateTime.UtcNow.AddHours(1));

        // Act
        var deleted = true;
        await context.UsingTenantScopeAsync(async scope => deleted = await scope.ServiceProvider.GetRequiredService<IJobManager>().DeleteAsync(runId));

        // Assert
        Assert.False(deleted);
        Assert.NotNull(await GetRunAsync(context, runId));
    }

    private static string NewKey() => Guid.NewGuid().ToString("n");

    private static async Task<string> EnqueueAsync(SiteContext context, TestJobParameters parameters, Action<JobRequest> configure = null)
    {
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<TestJob>(parameters, configure)).RunId;
        });

        return runId;
    }

    // Saves a run without telling this server to execute it, as if another server had queued it.
    private static async Task<string> CreateRunAsync(SiteContext context, TestJobParameters parameters, Action<JobRun> alter = null)
    {
        var run = new JobRun
        {
            RunId = Guid.NewGuid().ToString("n")[..26],
            JobName = TestJob.Name,
            Status = JobRunStatus.Queued,
            Trigger = JobTriggers.Code,
            Parameters = JObject.FromObject(parameters),
            CreatedUtc = DateTime.UtcNow,
            DueUtc = DateTime.UtcNow,
        };

        alter?.Invoke(run);

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(run, collection: JobRun.Collection));

        return run.RunId;
    }

    private static async Task<JobRun> GetRunAsync(SiteContext context, string runId)
    {
        JobRun run = null;

        await context.UsingTenantScopeAsync(async scope => run = await scope.ServiceProvider.GetRequiredService<IJobManager>().GetAsync(runId));

        return run;
    }

    private static async Task<int> CountRunsAsync(SiteContext context, System.Linq.Expressions.Expression<Func<JobRunIndex, bool>> predicate)
    {
        var count = 0;

        await context.UsingTenantScopeAsync(async scope =>
        {
            count = await scope.ServiceProvider.GetRequiredService<ISession>().QueryIndex(predicate, collection: JobRun.Collection).CountAsync();
        });

        return count;
    }

    private static async Task<JobRun> WaitForRunAsync(SiteContext context, string runId, Func<JobRun, bool> until = null, bool dispatch = false)
    {
        Assert.NotNull(runId);

        until ??= run => run.IsCompleted;

        var stopwatch = Stopwatch.StartNew();
        JobRun run = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            run = await GetRunAsync(context, runId);

            if (run is not null && until(run))
            {
                return run;
            }

            if (dispatch && run?.Status == JobRunStatus.Queued)
            {
                // What the background task of a server does every minute.
                await context.UsingTenantScopeAsync(scope =>
                {
                    scope.ServiceProvider.GetRequiredService<JobRunDispatcher>().Dispatch(runId);

                    return Task.CompletedTask;
                });
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"The run didn't reach the expected state; its status is {run?.Status}, its error '{run?.Error}'.");
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var features = (await featuresManager.GetAvailableFeaturesAsync())
                .Where(feature => feature.Id is "OrchardCore.Jobs")
                .ToList();

            await featuresManager.UpdateFeaturesAsync([], features, force: true);
        });
        await DataPipelineTestSites.WaitForFeaturesAsync(context);

        return context;
    }
}
