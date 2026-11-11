using System.Diagnostics;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using OrchardCore.Environment.Shell;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Modules.OrchardCore.DataPipelines;

namespace OrchardCore.Tests.Modules.OrchardCore.Jobs;

public sealed class JobsAdminPagesTests
{
    [Fact]
    public async Task AdminPages_Render_WithTheirBreadcrumbs()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var succeeded = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n"), Children = 1 }, "Succeeded run");
        var failed = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n"), FailCount = 1 }, "Failed run");

        string[] paths =
        [
            "Admin/Jobs/Runs",
            "Admin/Jobs/Runs?Options.Status=Active",
            $"Admin/Jobs/Runs?Options.Status=Failed&Options.JobName={TestJob.Name}",
            $"Admin/Jobs/Runs/Details/{succeeded.RunId}",
            $"Admin/Jobs/Runs/Details/{failed.RunId}",
            "Admin/Jobs",
            $"Admin/Jobs/RunWith/{TestJob.DeclaredScheduleName}",
            "Admin/Jobs/Schedules",
            "Admin/Jobs/Schedules/Create",
            $"Admin/Jobs/Schedules/Edit/{TestJob.DeclaredScheduleName}",
        ];

        foreach (var path in paths)
        {
            // Act
            using var document = await GetPageAsync(context, path);

            // Assert
            Assert.True(document.QuerySelectorAll("breadcrumb, breadcrumb-item, admin-list, admin-list-column").Length == 0, $"A tag helper of {path} isn't rendered.");
            Assert.NotNull(document.QuerySelector("h1.oc-breadcrumb-title"));
        }
    }

    [Fact]
    public async Task RunsPage_Runs_ShowsThemWithTheirStatus()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var succeeded = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n") }, "Succeeded run");
        var failed = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n"), FailCount = 1 }, "Failed run");

        // Act
        using var all = await GetPageAsync(context, "Admin/Jobs/Runs");
        using var onlyFailed = await GetPageAsync(context, "Admin/Jobs/Runs?Options.Status=Failed");

        // Assert
        var succeededRow = all.QuerySelector($"[data-job-run-id='{succeeded.RunId}']");
        var failedRow = all.QuerySelector($"[data-job-run-id='{failed.RunId}']");

        Assert.NotNull(succeededRow);
        Assert.NotNull(failedRow);
        Assert.Contains("Succeeded run", succeededRow.TextContent, StringComparison.Ordinal);
        Assert.NotNull(succeededRow.QuerySelector("[data-job-run-status='Succeeded']"));
        Assert.NotNull(failedRow.QuerySelector("[data-job-run-status='Failed']"));
        Assert.Contains("Test failure 1", failedRow.TextContent, StringComparison.Ordinal);
        Assert.Equal("false", succeededRow.GetAttribute("data-job-run-active"));

        Assert.Null(onlyFailed.QuerySelector($"[data-job-run-id='{succeeded.RunId}']"));
        Assert.NotNull(onlyFailed.QuerySelector($"[data-job-run-id='{failed.RunId}']"));
    }

    [Fact]
    public async Task DetailsPage_RunWithChildren_ShowsItsLogAndItsChildRuns()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var run = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n"), Children = 2 }, "Parent");

        // Act
        using var document = await GetPageAsync(context, $"Admin/Jobs/Runs/Details/{run.RunId}");

        // Assert
        var text = document.Body.TextContent;
        Assert.Contains("Execution 1", text, StringComparison.Ordinal);
        Assert.Contains("Child runs", text, StringComparison.Ordinal);
        Assert.Equal(2, document.QuerySelectorAll("table a[href*='/Jobs/Runs/Details/']").Length);
        Assert.Contains(run.NodeId, text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task States_KnownRuns_ReturnsTheirSignature()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var run = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n") }, "Polled");

        // Act
        using var response = await context.Client.GetAsync($"Admin/Jobs/Runs/States?runIds={run.RunId}&runIds=unknown", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

        // Assert
        var state = Assert.Single(json.RootElement.GetProperty("runs").EnumerateArray());
        Assert.Equal(run.RunId, state.GetProperty("runId").GetString());
        Assert.False(state.GetProperty("isActive").GetBoolean());
        Assert.StartsWith("Succeeded|1|100|", state.GetProperty("signature").GetString(), StringComparison.Ordinal);
        Assert.False(string.IsNullOrEmpty(json.RootElement.GetProperty("latest").GetString()));
    }

    [Fact]
    public async Task JobsPage_RegisteredJobs_ListsThemWithTheirSchedule()
    {
        // Arrange
        using var context = await CreateContextAsync();

        // Act
        using var document = await GetPageAsync(context, "Admin/Jobs");

        // Assert
        var text = document.Body.TextContent;
        Assert.Contains(TestJob.Name, text, StringComparison.Ordinal);
        Assert.Contains(TestJob.DeclaredScheduleName, text, StringComparison.Ordinal);
        Assert.Contains("0 0 1 1 *", text, StringComparison.Ordinal);

        // Only a job that declares a schedule, or allows it, can be run from the admin.
        Assert.NotNull(document.QuerySelector($"a[href*='/Jobs/Run/{TestJob.DeclaredScheduleName}']"));
        Assert.Null(document.QuerySelector($"a[href*='/Jobs/Run/{TestJob.Name}?']"));
    }

    [Fact]
    public async Task RunWithPage_Job_ShowsItsTitleAsAFieldNotInThePageTitle()
    {
        // Arrange
        using var context = await CreateContextAsync();

        // Act
        using var document = await GetPageAsync(context, $"Admin/Jobs/RunWith/{TestJob.ManualName}");

        // Assert
        Assert.Equal("Run with Parameters", document.QuerySelector("h1.oc-breadcrumb-title").TextContent.Trim());
        Assert.Equal(TestJob.ManualName, document.QuerySelector("input#JobTitle").GetAttribute("value"));
    }

    [Fact]
    public async Task SchedulesPage_ScheduleWithoutName_ShowsItsJobOnce()
    {
        // Arrange
        using var context = await CreateContextAsync();

        // Act
        using var document = await GetPageAsync(context, "Admin/Jobs/Schedules");

        // Assert
        var row = document.QuerySelector($"a[href*='/Jobs/Schedules/Edit/{TestJob.DeclaredScheduleName}']")?.Closest("li");

        Assert.NotNull(row);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(row.TextContent, TestJob.DeclaredScheduleName));
    }

    [Fact]
    public async Task SchedulesPage_Schedule_DescribesItsCronExpression()
    {
        // Arrange
        using var context = await CreateContextAsync();

        // Act
        using var schedules = await GetPageAsync(context, "Admin/Jobs/Schedules");
        using var jobs = await GetPageAsync(context, "Admin/Jobs");

        // Assert
        var row = schedules.QuerySelector($"a[href*='/Jobs/Schedules/Edit/{TestJob.DeclaredScheduleName}']")?.Closest("li");

        Assert.NotNull(row);
        Assert.Equal("0 0 1 1 *", row.QuerySelector(".cron-expression code").TextContent);
        Assert.Contains("January", row.QuerySelector(".cron-expression-description").TextContent, StringComparison.Ordinal);
        Assert.DoesNotContain("(", row.QuerySelector(".cron-expression-description").TextContent, StringComparison.Ordinal);

        // The same clock time is another instant in another time zone, which the tooltip tells.
        Assert.Contains("time zone of the site", row.QuerySelector(".cron-expression").GetAttribute("title"), StringComparison.Ordinal);
        Assert.False(string.IsNullOrEmpty(row.QuerySelector(".cron-expression").GetAttribute("data-cron-time-zone")));
        Assert.DoesNotContain("{0}", row.QuerySelector(".cron-expression").GetAttribute("title"), StringComparison.Ordinal);

        // The list of jobs has no room for the description, which is the tooltip of the expression.
        Assert.Contains(jobs.QuerySelectorAll(".cron-expression"), expression =>
            expression.QuerySelector("code").TextContent == "0 0 1 1 *"
            && expression.GetAttribute("title").Contains("January", StringComparison.Ordinal)
            && expression.GetAttribute("title").Contains("time zone of the site", StringComparison.Ordinal)
            && expression.QuerySelector(".cron-expression-description") is null);
    }

    [Fact]
    public async Task SchedulesPage_ScheduleWithATimeZone_ShowsIt()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var schedule = new JobSchedule { Name = "Paris", JobName = TestJob.ManualName, Cron = "0 3 * * *", TimeZoneId = "Europe/Paris" };

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IJobScheduleManager>().SaveAsync(schedule));

        // Act
        using var schedules = await GetPageAsync(context, "Admin/Jobs/Schedules");
        using var edit = await GetPageAsync(context, $"Admin/Jobs/Schedules/Edit/{schedule.ScheduleId}");

        // Assert
        var row = schedules.QuerySelector($"a[href*='/Jobs/Schedules/Edit/{schedule.ScheduleId}']")?.Closest("li");

        Assert.NotNull(row);
        Assert.Equal("Europe/Paris", row.QuerySelector(".cron-expression").GetAttribute("data-cron-time-zone"));
        Assert.Equal("In the Europe/Paris time zone", row.QuerySelector(".cron-expression").GetAttribute("title"));

        // The description tells the time zone, as it is not the one of the site: winter or summer time.
        Assert.Matches(@"\((CET|CEST)\)$", row.QuerySelector(".cron-expression-description").TextContent.Trim());

        Assert.Equal("Europe/Paris", edit.QuerySelector("select#TimeZoneId option[selected]").GetAttribute("value"));
    }

    [Fact]
    public async Task EditSchedulePage_Schedule_EditsItsCronExpressionWithTheSharedEditor()
    {
        // Arrange
        using var context = await CreateContextAsync();

        // The schedule a job declares is created when the schedules are listed.
        using var schedules = await GetPageAsync(context, "Admin/Jobs/Schedules");

        // Act
        using var document = await GetPageAsync(context, $"Admin/Jobs/Schedules/Edit/{TestJob.DeclaredScheduleName}");

        // Assert
        var editor = document.QuerySelector("[data-cron-editor]");

        Assert.NotNull(editor);
        Assert.Equal("0 0 1 1 *", editor.QuerySelector("input#Cron[name='Cron'][data-cron-input]").GetAttribute("value"));
        Assert.Contains("January", editor.QuerySelector("[data-cron-description]").TextContent, StringComparison.Ordinal);
        Assert.EndsWith("/Admin/CronExpression/Describe", editor.GetAttribute("data-cron-describe-url"), StringComparison.Ordinal);
        Assert.Equal("false", editor.GetAttribute("data-cron-utc"));

        // The time zone of the site is the first choice, which the schedule follows, and it can be selected as any
        // other one. The next occurrences follow the selection.
        var timeZones = document.QuerySelectorAll("select#TimeZoneId option");

        Assert.Equal("TimeZoneId", editor.GetAttribute("data-cron-time-zone-input"));
        Assert.Equal(string.Empty, timeZones[0].GetAttribute("value"));
        Assert.StartsWith("Time zone of the site, currently ", timeZones[0].TextContent, StringComparison.Ordinal);

        // No other time zone is selected, so the first one is.
        Assert.Null(document.QuerySelector("select#TimeZoneId option[selected]"));
        Assert.Contains(timeZones, option => option.GetAttribute("value") == "Europe/Paris");
        Assert.NotNull(editor.QuerySelector("a[data-cron-guru][href^='https://crontab.guru/']"));

        // The schedule the job declares is offered after the common ones.
        var presets = editor.QuerySelectorAll("[data-cron-preset]").Select(preset => preset.GetAttribute("data-cron-preset")).ToArray();

        Assert.Contains("* * * * *", presets);
        Assert.Equal("0 0 1 1 *", presets[^1]);
        Assert.Contains(document.Scripts, script => script.Source?.Contains("cron-expression-editor", StringComparison.Ordinal) == true);
    }

    [Fact]
    public async Task DetailsPage_FailedRun_ShowsTheExceptionToThoseWhoManageJobs()
    {
        // Arrange
        using var context = await CreateContextAsync();
        var failed = await RunAsync(context, new TestJobParameters { Key = Guid.NewGuid().ToString("n"), FailCount = 1 }, "Failed run");

        // Act
        using var document = await GetPageAsync(context, $"Admin/Jobs/Runs/Details/{failed.RunId}");

        // Assert
        Assert.Contains(nameof(InvalidOperationException), document.QuerySelector(".job-run-details details pre").TextContent, StringComparison.Ordinal);
    }

    private static async Task<JobRun> RunAsync(SiteContext context, TestJobParameters parameters, string title)
    {
        string runId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            runId = (await scope.ServiceProvider.GetRequiredService<IJobManager>().EnqueueAsync<TestJob>(parameters, request => request.Title = title)).RunId;
        });

        var stopwatch = Stopwatch.StartNew();
        JobRun run = null;

        while (stopwatch.Elapsed < TimeSpan.FromSeconds(90))
        {
            await context.UsingTenantScopeAsync(async scope => run = await scope.ServiceProvider.GetRequiredService<IJobManager>().GetAsync(runId));

            if (run?.IsCompleted == true)
            {
                return run;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"The run didn't end; its status is {run?.Status}.");
    }

    private static async Task<IDocument> GetPageAsync(SiteContext context, string path)
    {
        using var response = await context.Client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.True(response.IsSuccessStatusCode, $"{path} returned {(int)response.StatusCode}.");

        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
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
