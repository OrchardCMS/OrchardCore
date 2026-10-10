using Microsoft.Playwright;
using OrchardCore.BackgroundTasks;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the execution state shown by the OrchardCore.BackgroundTasks admin pages, and the on-demand runs.
// The fixture registers two disabled test tasks, so that they only run when requested from the admin, and
// shortens the polling time of the background service so that the tasks of the tenant are loaded quickly.
public sealed class BackgroundTasksTests : CmsTestBase<BackgroundTasksTestsFixture>, IClassFixture<BackgroundTasksTestsFixture>
{
    private static readonly float _runTimeout = 60_000;

    public BackgroundTasksTests(BackgroundTasksTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task RunNow_SucceedingTask_ShowsLastRunOnListAndDetails()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        var row = await GotoLoadedTaskRowAsync(page, SucceedingTestBackgroundTask.TaskName);

        // The task is disabled, so it is not scheduled, but it can still be run on demand.
        await Assertions.Expect(row.Locator("[data-next-run]")).ToContainTextAsync("not scheduled");

        await row.Locator("a[data-run-task]").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(page.Locator(".message-success")).ToContainTextAsync("has been queued and will run shortly");

        // The page polls the status of the queued task and reloads once it ran.
        row = page.Locator($"li[data-task-name='{SucceedingTestBackgroundTask.TaskName}']");
        await Assertions.Expect(row.Locator("[data-last-run] [data-run-result='Succeeded']"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = _runTimeout });
        await Assertions.Expect(row).ToHaveAttributeAsync("data-task-status", "Idle");
        await Assertions.Expect(row.Locator("a[data-run-task]")).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("\\bdisabled\\b"));

        // The details page shows the run, and runs the task again from its own button.
        await row.Locator("a", new LocatorLocatorOptions { HasText = "Details" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var details = page.Locator(".background-task-details");
        await Assertions.Expect(details.Locator("[data-last-run] [data-run-result='Succeeded']")).ToBeVisibleAsync();
        await Assertions.Expect(details.Locator("[data-run-count]")).ToHaveTextAsync("1");
        await Assertions.Expect(details.Locator("[data-failure-count]")).ToHaveTextAsync("0");
        await Assertions.Expect(details.Locator("[data-recent-runs] tbody tr")).ToHaveCountAsync(1);
        await Assertions.Expect(details.Locator("[data-last-error]")).ToHaveCountAsync(0);

        await details.Locator("button[data-run-task]").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // The run redirects back to the details page, which reloads once the task ran again.
        Assert.Contains("/Admin/BackgroundTasks/Details/", page.Url);
        await Assertions.Expect(page.Locator(".background-task-details [data-run-count]"))
            .ToHaveTextAsync("2", new LocatorAssertionsToHaveTextOptions { Timeout = _runTimeout });

        var recentRuns = page.Locator(".background-task-details [data-recent-runs] tbody tr");
        await Assertions.Expect(recentRuns).ToHaveCountAsync(2);
        await Assertions.Expect(recentRuns.First).ToContainTextAsync("Run now");

        await page.CloseAsync();
    }

    [Fact]
    public async Task RunNow_FailingTask_ShowsErrorOnListAndDetails()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        var row = await GotoLoadedTaskRowAsync(page, FailingTestBackgroundTask.TaskName);

        await row.Locator("a[data-run-task]").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        row = page.Locator($"li[data-task-name='{FailingTestBackgroundTask.TaskName}']");
        await Assertions.Expect(row.Locator("[data-last-run] [data-run-result='Failed']"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Timeout = _runTimeout });
        await Assertions.Expect(row.Locator("[data-last-error]")).ToContainTextAsync(FailingTestBackgroundTask.ErrorMessage);

        // The status filter lists the tasks whose last run failed.
        await page.GotoAndAssertOkAsync("/Admin/BackgroundTasks?Options.Status=failed");
        await Assertions.Expect(page.Locator($"li[data-task-name='{FailingTestBackgroundTask.TaskName}']")).ToHaveCountAsync(1);
        await Assertions.Expect(page.Locator($"li[data-task-name='{SucceedingTestBackgroundTask.TaskName}']")).ToHaveCountAsync(0);

        await page.GotoAndAssertOkAsync($"/Admin/BackgroundTasks/Details/{FailingTestBackgroundTask.TaskName}");

        var details = page.Locator(".background-task-details");
        await Assertions.Expect(details.Locator("[data-last-error] [data-error-message]")).ToHaveTextAsync(FailingTestBackgroundTask.ErrorMessage);
        await Assertions.Expect(details.Locator("[data-last-error] [data-error-details]")).ToContainTextAsync(nameof(InvalidOperationException));
        await Assertions.Expect(details.Locator("[data-failure-count]")).Not.ToHaveTextAsync("0");

        var lastRun = details.Locator("[data-recent-runs] tbody tr").First;
        await Assertions.Expect(lastRun.Locator("[data-run-result='Failed']")).ToBeVisibleAsync();
        await Assertions.Expect(lastRun).ToContainTextAsync(FailingTestBackgroundTask.ErrorMessage);

        await page.CloseAsync();
    }

    /// <summary>
    /// Opens the background tasks list until the background service loaded the given task, which happens on its
    /// first polling after the tenant is set up.
    /// </summary>
    private static async Task<ILocator> GotoLoadedTaskRowAsync(IPage page, string taskName)
    {
        var row = page.Locator($"li[data-task-name='{taskName}']");
        var deadline = DateTime.UtcNow.AddMilliseconds(_runTimeout);

        while (true)
        {
            await page.GotoAndAssertOkAsync("/Admin/BackgroundTasks");
            await Assertions.Expect(row).ToHaveCountAsync(1);

            if (await row.GetAttributeAsync("data-task-status") == "Idle")
            {
                return row;
            }

            Assert.True(DateTime.UtcNow < deadline, $"The background service didn't load the task '{taskName}'.");
            await page.WaitForTimeoutAsync(1000);
        }
    }
}

[BackgroundTask(
    Title = "Functional Test Succeeding Task",
    Schedule = "0 0 1 1 *",
    Enable = false,
    Description = "A task that only runs on demand in the functional tests.")]
public sealed class SucceedingTestBackgroundTask : IBackgroundTask
{
    public static readonly string TaskName = typeof(SucceedingTestBackgroundTask).FullName;

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => Task.Delay(200, cancellationToken);
}

[BackgroundTask(
    Title = "Functional Test Failing Task",
    Schedule = "0 0 1 1 *",
    Enable = false,
    Description = "A task that fails on purpose when it runs on demand in the functional tests.")]
public sealed class FailingTestBackgroundTask : IBackgroundTask
{
    public const string ErrorMessage = "The functional test task failed on purpose.";

    public static readonly string TaskName = typeof(FailingTestBackgroundTask).FullName;

    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        => throw new InvalidOperationException(ErrorMessage);
}
