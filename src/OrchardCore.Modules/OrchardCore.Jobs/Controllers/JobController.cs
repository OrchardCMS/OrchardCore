using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
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
using ISession = YesSql.ISession;

namespace OrchardCore.Jobs.Controllers;

[Admin("Jobs/{action}/{name?}", "Jobs{action}")]
public sealed class JobController : Controller
{
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

    public JobController(
        IAuthorizationService authorizationService,
        ISession session,
        IJobManager jobManager,
        IJobScheduleManager scheduleManager,
        JobDisplayService displayService,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IStringLocalizer<JobController> stringLocalizer,
        IHtmlLocalizer<JobController> htmlLocalizer)
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

    [Admin("Jobs", "Jobs")]
    public async Task<IActionResult> Index(string search, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ViewJobs))
        {
            return Forbid();
        }

        var canManage = await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs);
        var schedules = await _scheduleManager.ListAsync();

        var entries = _displayService.GetJobs().Select(job => new JobEntry
        {
            Name = job.Name,
            Title = _displayService.GetTitle(job),
            Description = _displayService.GetDescription(job),
            CanRunManually = job.CanRunManually,
            CanManage = canManage,
            Schedules = schedules.Where(schedule => string.Equals(schedule.JobName, job.Name, StringComparison.OrdinalIgnoreCase)).ToList(),
        });

        if (!string.IsNullOrWhiteSpace(search))
        {
            entries = entries.Where(entry => entry.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                || (entry.Description is not null && entry.Description.Contains(search, StringComparison.CurrentCultureIgnoreCase)));
        }

        var all = entries.ToList();
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(search))
        {
            routeData.Values.TryAdd(nameof(JobIndexViewModel.Search), search);
        }

        var pager = new Pager(pagerParameters, _pagerOptions);
        var page = all.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList();

        // The most recent run of the jobs of the page.
        foreach (var entry in page)
        {
            var jobName = JobRunIndexProvider.Truncate(entry.Name, JobRunIndex.MaxJobNameLength);

            entry.LastRun = await _session.Query<JobRun, JobRunIndex>(index => index.JobName == jobName, collection: JobRun.Collection)
                .OrderByDescending(index => index.CreatedUtc)
                .FirstOrDefaultAsync();
        }

        var model = new JobIndexViewModel
        {
            Search = search,

            // The AdminList shape renders the jobs with the configured layout (List, Grid, ...).
            List = new AdminListContext<JobEntry>
            {
                Source = page,
                Pager = await _shapeFactory.PagerAsync(pager, all.Count, routeData),
                ShowSelectAll = false,
            },
        };

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(JobIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { nameof(JobIndexViewModel.Search), model.Search },
        });

    /// <summary>
    /// Queues a run of a job without parameters.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Run(string name, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var job = _displayService.Find(name);

        if (job is null || !job.CanRunManually)
        {
            return NotFound();
        }

        var run = await _jobManager.EnqueueAsync(job.Name, null, request =>
        {
            request.Trigger = JobTriggers.Manual;
            request.TriggeredBy = User.Identity?.Name;
        });

        await _notifier.SuccessAsync(H["A run of the job was queued. <a href=\"{0}\">View the run</a>.", Url.Action(nameof(JobRunController.Details), "JobRun", new { runId = run.RunId })]);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.LocalRedirect(returnUrl, true);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Asks for the parameters of a run of a job.
    /// </summary>
    public async Task<IActionResult> RunWith(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var job = _displayService.Find(name);

        if (job is null || !job.CanRunManually)
        {
            return NotFound();
        }

        return View(new JobRunNowViewModel
        {
            Name = job.Name,
            JobTitle = _displayService.GetTitle(job),
            JobDescription = _displayService.GetDescription(job),
        });
    }

    [HttpPost, ActionName(nameof(RunWith))]
    public async Task<IActionResult> RunWithPOST(JobRunNowViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var job = _displayService.Find(model.Name);

        if (job is null || !job.CanRunManually)
        {
            return NotFound();
        }

        JsonObject parameters = null;

        if (!string.IsNullOrWhiteSpace(model.Parameters) && !TryParseParameters(model.Parameters, out parameters))
        {
            ModelState.AddModelError(nameof(JobRunNowViewModel.Parameters), S["The parameters must be a JSON object, such as {0}.", "{ \"name\": \"value\" }"]);
        }

        if (!ModelState.IsValid)
        {
            model.JobTitle = _displayService.GetTitle(job);
            model.JobDescription = _displayService.GetDescription(job);

            return View(model);
        }

        var run = await _jobManager.EnqueueAsync(job.Name, parameters, request =>
        {
            request.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();
            request.Trigger = JobTriggers.Manual;
            request.TriggeredBy = User.Identity?.Name;
        });

        await _notifier.SuccessAsync(H["A run of the job was queued."]);

        return RedirectToAction(nameof(JobRunController.Details), "JobRun", new { runId = run.RunId });
    }

    internal static bool TryParseParameters(string json, out JsonObject parameters)
    {
        parameters = null;

        try
        {
            parameters = JsonNode.Parse(json) as JsonObject;
        }
        catch (System.Text.Json.JsonException)
        {
        }

        return parameters is not null;
    }
}
