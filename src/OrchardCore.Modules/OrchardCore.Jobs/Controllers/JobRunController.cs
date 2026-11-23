using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Jobs.ViewModels;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using YesSql;
using YesSql.Services;
using ISession = YesSql.ISession;

namespace OrchardCore.Jobs.Controllers;

[Admin("Jobs/Runs/{action}/{runId?}", "JobRuns{action}")]
public sealed class JobRunController : Controller
{
    private const string _optionsStatus = $"{nameof(JobRunIndexViewModel.Options)}.{nameof(JobRunIndexOptions.Status)}";
    private const string _optionsJobName = $"{nameof(JobRunIndexViewModel.Options)}.{nameof(JobRunIndexOptions.JobName)}";

    // The status of the runs that didn't end, which the status filter groups.
    private const string _activeStatus = "Active";

    // Sent by the admin pages to refresh the displayed runs.
    private const string _refreshHeader = "X-Jobs-Refresh";

    private const int MaxPolledRuns = 100;

    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly IJobManager _jobManager;
    private readonly IJobScheduleManager _scheduleManager;
    private readonly JobDisplayService _displayService;
    private readonly PagerOptions _pagerOptions;
    private readonly IShapeFactory _shapeFactory;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public JobRunController(
        IAuthorizationService authorizationService,
        ISession session,
        IJobManager jobManager,
        IJobScheduleManager scheduleManager,
        JobDisplayService displayService,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IStringLocalizer<JobRunController> stringLocalizer,
        IHtmlLocalizer<JobRunController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _session = session;
        _jobManager = jobManager;
        _scheduleManager = scheduleManager;
        _displayService = displayService;
        _pagerOptions = pagerOptions.Value;
        _shapeFactory = shapeFactory;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("Jobs/Runs", "JobRuns")]
    public async Task<IActionResult> Index(JobRunIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ViewJobs))
        {
            return Forbid();
        }

        options ??= new JobRunIndexOptions();

        // The child runs are shown with their parent run.
        var query = _session.Query<JobRun, JobRunIndex>(index => index.ParentRunId == null, collection: JobRun.Collection);

        if (string.Equals(options.Status, _activeStatus, StringComparison.OrdinalIgnoreCase))
        {
            var queued = nameof(JobRunStatus.Queued);
            var running = nameof(JobRunStatus.Running);
            var waiting = nameof(JobRunStatus.Waiting);

            query = query.Where(index => index.Status == queued || index.Status == running || index.Status == waiting);
        }
        else if (Enum.TryParse<JobRunStatus>(options.Status, ignoreCase: true, out var status))
        {
            var statusName = status.ToString();

            query = query.Where(index => index.Status == statusName);
        }

        if (!string.IsNullOrEmpty(options.JobName))
        {
            var jobName = JobRunIndexProvider.Truncate(options.JobName, JobRunIndex.MaxJobNameLength);

            query = query.Where(index => index.JobName == jobName);
        }

        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Status))
        {
            routeData.Values.TryAdd(_optionsStatus, options.Status);
        }

        if (!string.IsNullOrEmpty(options.JobName))
        {
            routeData.Values.TryAdd(_optionsJobName, options.JobName);
        }

        var pager = new Pager(pagerParameters, _pagerOptions);
        var count = await query.CountAsync();
        var runs = (await query.OrderByDescending(index => index.CreatedUtc).Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync()).ToList();
        var pagerShape = await _shapeFactory.PagerAsync(pager, count, routeData);

        options.Statuses =
        [
            new SelectListItem(S["All"], string.Empty),
            new SelectListItem(S["Active"], _activeStatus),
            new SelectListItem(S["Queued"], nameof(JobRunStatus.Queued)),
            new SelectListItem(S["Running"], nameof(JobRunStatus.Running)),
            new SelectListItem(S["Waiting for child runs"], nameof(JobRunStatus.Waiting)),
            new SelectListItem(S["Succeeded"], nameof(JobRunStatus.Succeeded)),
            new SelectListItem(S["Failed"], nameof(JobRunStatus.Failed)),
            new SelectListItem(S["Canceled"], nameof(JobRunStatus.Canceled)),
        ];

        options.Jobs =
        [
            new SelectListItem(S["All jobs"], string.Empty),
            .. _displayService.GetJobs().Select(job => new SelectListItem(_displayService.GetTitle(job), job.Name)),
        ];

        var canManage = await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs);

        options.BulkActions = canManage
            ?
            [
                new SelectListItem(S["Cancel"], nameof(JobRunBulkAction.Cancel)),
                new SelectListItem(S["Run again"], nameof(JobRunBulkAction.Rerun)),
                new SelectListItem(S["Delete"], nameof(JobRunBulkAction.Delete)),
            ]
            : [];

        // The status and job filters render at the end of the toolbar, next to the bulk actions.
        var filters = await _shapeFactory.CreateAsync("JobRunsFilters", Arguments.From(new
        {
            Options = options,
        }));

        var model = new JobRunIndexViewModel
        {
            Options = options,

            // The AdminList shape renders the runs with the configured layout (List, Grid, ...).
            List = new AdminListContext<JobRun>
            {
                Source = runs,
                ToolbarActions = filters,
                Pager = pagerShape,
                ShowSelectAll = canManage,
            },
        };

        return ViewOrRefresh(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(JobRunIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsStatus, model.Options.Status },
            { _optionsJobName, model.Options.JobName },
        });

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> IndexBulkActionPOST(JobRunIndexOptions options, IEnumerable<string> runIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var selected = runIds?.Where(runId => !string.IsNullOrEmpty(runId)).Distinct().ToList() ?? [];

        if (selected.Count == 0)
        {
            await _notifier.WarningAsync(H["Please select one or more runs."]);

            return RedirectToAction(nameof(Index));
        }

        var count = 0;

        foreach (var runId in selected)
        {
            var done = options.BulkAction switch
            {
                JobRunBulkAction.Cancel => await _jobManager.CancelAsync(runId, User.Identity?.Name),
                JobRunBulkAction.Rerun => await _jobManager.RerunAsync(runId, User.Identity?.Name) is not null,
                JobRunBulkAction.Delete => await _jobManager.DeleteAsync(runId),
                _ => false,
            };

            if (done)
            {
                count++;
            }
        }

        switch (options.BulkAction)
        {
            case JobRunBulkAction.Cancel:
                await _notifier.SuccessAsync(H.Plural(count, "1 run was canceled.", "{0} runs were canceled."));
                break;
            case JobRunBulkAction.Rerun:
                await _notifier.SuccessAsync(H.Plural(count, "1 run was queued again.", "{0} runs were queued again."));
                break;
            case JobRunBulkAction.Delete:
                await _notifier.SuccessAsync(H.Plural(count, "1 run was deleted.", "{0} runs were deleted."));
                break;
        }

        if (count < selected.Count)
        {
            await _notifier.WarningAsync(H["Some of the selected runs were left as they are: a run that ended can't be canceled, and a run that didn't end can't be run again or deleted."]);
        }

        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsStatus, options.Status },
            { _optionsJobName, options.JobName },
        });
    }

    public async Task<IActionResult> Details(string runId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ViewJobs))
        {
            return Forbid();
        }

        var run = await _jobManager.GetAsync(runId);

        if (run is null)
        {
            return NotFound();
        }

        var model = new JobRunDetailsViewModel
        {
            Run = run,
            JobTitle = _displayService.GetTitle(run.JobName),
            IsJobRegistered = _displayService.Find(run.JobName) is not null,
            Parent = string.IsNullOrEmpty(run.ParentRunId) ? null : await _jobManager.GetAsync(run.ParentRunId),
            Schedule = string.IsNullOrEmpty(run.ScheduleId) ? null : await _scheduleManager.GetAsync(run.ScheduleId),
            Children = await _jobManager.ListChildrenAsync(run.RunId),
            CanManage = await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs),
        };

        return ViewOrRefresh(model);
    }

    /// <summary>
    /// Returns what tells whether the displayed runs changed, polled by the admin pages. Whichever server answers,
    /// the answer is the same, as the runs are read from the database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> States([FromQuery] string[] runIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ViewJobs))
        {
            return Forbid();
        }

        var ids = (runIds ?? []).Where(runId => !string.IsNullOrEmpty(runId)).Distinct().Take(MaxPolledRuns).ToArray();

        var runs = ids.Length == 0
            ? []
            : await _session.Query<JobRun, JobRunIndex>(index => index.RunId.IsIn(ids), collection: JobRun.Collection).ListAsync();

        // A run that was queued since the page was rendered may have to be added to it.
        var latest = await _session.QueryIndex<JobRunIndex>(index => index.ParentRunId == null, collection: JobRun.Collection)
            .OrderByDescending(index => index.CreatedUtc)
            .FirstOrDefaultAsync();

        return Json(new
        {
            Latest = latest?.RunId,
            Runs = runs.Select(run => new
            {
                run.RunId,
                Signature = JobDisplayService.GetSignature(run),
                IsActive = !run.IsCompleted,
            }),
        });
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(string runId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        if (await _jobManager.CancelAsync(runId, User.Identity?.Name))
        {
            await _notifier.SuccessAsync(H["The cancellation of the run was requested. A running run stops within a few seconds."]);
        }
        else
        {
            await _notifier.WarningAsync(H["The run can't be canceled, as it already ended."]);
        }

        return RedirectToReturnUrlOrDetails(returnUrl, runId);
    }

    [HttpPost]
    public async Task<IActionResult> Rerun(string runId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var run = await _jobManager.RerunAsync(runId, User.Identity?.Name);

        if (run is null)
        {
            await _notifier.WarningAsync(H["The run can't be run again: it didn't end yet, or its job is not available anymore."]);

            return RedirectToReturnUrlOrDetails(returnUrl, runId);
        }

        await _notifier.SuccessAsync(H["A new run was queued."]);

        // A list goes back to itself, where the new run shows up; the page of a run opens the new one.
        return RedirectToReturnUrlOrDetails(returnUrl, run.RunId);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string runId, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        if (await _jobManager.DeleteAsync(runId))
        {
            await _notifier.SuccessAsync(H["The run was deleted."]);
        }
        else
        {
            await _notifier.WarningAsync(H["The run can't be deleted, as it didn't end yet. Cancel it first."]);

            return RedirectToReturnUrlOrDetails(returnUrl, runId);
        }

        // The page of the run doesn't exist anymore.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.Contains(runId, StringComparison.OrdinalIgnoreCase))
        {
            return this.LocalRedirect(returnUrl, true);
        }

        return RedirectToAction(nameof(Index));
    }

    private IActionResult ViewOrRefresh(object model)
        => Request.Headers.ContainsKey(_refreshHeader) ? PartialView(model) : View(model);

    private IActionResult RedirectToReturnUrlOrDetails(string returnUrl, string runId)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.LocalRedirect(returnUrl, true);
        }

        return RedirectToAction(nameof(Details), new { runId });
    }
}
