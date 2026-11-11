using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Jobs.ViewModels;

namespace OrchardCore.Jobs.Controllers;

[Admin("Jobs/Schedules/{action}/{scheduleId?}", "JobSchedules{action}")]
public sealed class JobScheduleController : Controller
{
    private static readonly JsonSerializerOptions _indented = new() { WriteIndented = true };

    private readonly IAuthorizationService _authorizationService;
    private readonly IJobManager _jobManager;
    private readonly IJobScheduleManager _scheduleManager;
    private readonly JobDisplayService _displayService;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public JobScheduleController(
        IAuthorizationService authorizationService,
        IJobManager jobManager,
        IJobScheduleManager scheduleManager,
        JobDisplayService displayService,
        INotifier notifier,
        IStringLocalizer<JobScheduleController> stringLocalizer,
        IHtmlLocalizer<JobScheduleController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _jobManager = jobManager;
        _scheduleManager = scheduleManager;
        _displayService = displayService;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("Jobs/Schedules", "JobSchedules")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ViewJobs))
        {
            return Forbid();
        }

        var canManage = await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs);

        var model = new JobScheduleIndexViewModel
        {
            CanCreate = canManage && GetSchedulableJobs().Count > 0,

            // The AdminList shape renders the schedules with the configured layout (List, Grid, ...).
            List = new AdminListContext<JobSchedule>
            {
                Source = await _scheduleManager.ListAsync(),
                ShowSelectAll = false,
            },
        };

        return View(model);
    }

    public async Task<IActionResult> Create(string jobName)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var model = new JobScheduleEditViewModel { IsNew = true, JobName = jobName, Cron = "0 0 * * *" };

        Populate(model, null);

        return View(nameof(Edit), model);
    }

    [HttpPost, ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePOST(JobScheduleEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        model.IsNew = true;

        var job = _displayService.Find(model.JobName);

        if (!string.IsNullOrEmpty(model.JobName) && (job is null || !job.CanRunManually))
        {
            ModelState.AddModelError(nameof(JobScheduleEditViewModel.JobName), S["The job can't be scheduled."]);
        }

        var parameters = Validate(model);

        if (!ModelState.IsValid)
        {
            Populate(model, null);

            return View(nameof(Edit), model);
        }

        await _scheduleManager.SaveAsync(new JobSchedule
        {
            JobName = job.Name,
            Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim(),
            Cron = model.Cron.Trim(),
            IsEnabled = model.IsEnabled,
            Parameters = parameters,
        });

        await _notifier.SuccessAsync(H["The schedule was created."]);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string scheduleId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var schedule = await _scheduleManager.GetAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        var model = new JobScheduleEditViewModel
        {
            ScheduleId = schedule.ScheduleId,
            JobName = schedule.JobName,
            Name = schedule.Name,
            Cron = schedule.Cron,
            IsEnabled = schedule.IsEnabled,
            Parameters = schedule.Parameters?.ToJsonString(_indented),
        };

        Populate(model, schedule);

        return View(model);
    }

    [HttpPost, ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPOST(string scheduleId, JobScheduleEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var schedule = await _scheduleManager.GetAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        // The job of a schedule doesn't change.
        model.ScheduleId = schedule.ScheduleId;
        model.JobName = schedule.JobName;
        ModelState.Remove(nameof(JobScheduleEditViewModel.JobName));

        if (_displayService.Find(schedule.JobName) is null)
        {
            ModelState.AddModelError(string.Empty, S["The job of this schedule is not available anymore, such as when its feature is disabled."]);
        }

        var parameters = Validate(model);

        if (!ModelState.IsValid)
        {
            Populate(model, schedule);

            return View(model);
        }

        schedule.Name = string.IsNullOrWhiteSpace(model.Name) ? null : model.Name.Trim();
        schedule.Cron = model.Cron.Trim();
        schedule.IsEnabled = model.IsEnabled;
        schedule.Parameters = parameters;

        await _scheduleManager.SaveAsync(schedule);
        await _notifier.SuccessAsync(H["The schedule was saved."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Enable(string scheduleId)
        => await SetEnabledAsync(scheduleId, true);

    [HttpPost]
    public async Task<IActionResult> Disable(string scheduleId)
        => await SetEnabledAsync(scheduleId, false);

    /// <summary>
    /// Queues a run of the job of a schedule, with its parameters, without changing its next occurrence.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Run(string scheduleId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var schedule = await _scheduleManager.GetAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        if (_displayService.Find(schedule.JobName) is null)
        {
            await _notifier.WarningAsync(H["The job of this schedule is not available anymore, such as when its feature is disabled."]);

            return RedirectToAction(nameof(Index));
        }

        var run = await _jobManager.EnqueueAsync(schedule.JobName, schedule.Parameters, request =>
        {
            request.Title = schedule.Name;
            request.Trigger = JobTriggers.Manual;
            request.TriggeredBy = User.Identity?.Name;
            request.ScheduleId = schedule.ScheduleId;
        });

        await _notifier.SuccessAsync(H["A run of the job was queued. <a href=\"{0}\">View the run</a>.", Url.Action(nameof(JobRunController.Details), "JobRun", new { runId = run.RunId })]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string scheduleId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        if (await _scheduleManager.DeleteAsync(scheduleId))
        {
            await _notifier.SuccessAsync(H["The schedule was deleted."]);
        }
        else
        {
            await _notifier.WarningAsync(H["The schedule can't be deleted, as its job declares it. Disable it instead."]);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> SetEnabledAsync(string scheduleId, bool enabled)
    {
        if (!await _authorizationService.AuthorizeAsync(User, JobsPermissions.ManageJobs))
        {
            return Forbid();
        }

        var schedule = await _scheduleManager.GetAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        if (_displayService.Find(schedule.JobName) is null)
        {
            await _notifier.WarningAsync(H["The job of this schedule is not available anymore, such as when its feature is disabled."]);

            return RedirectToAction(nameof(Index));
        }

        schedule.IsEnabled = enabled;
        await _scheduleManager.SaveAsync(schedule);

        await _notifier.SuccessAsync(enabled ? H["The schedule was enabled."] : H["The schedule was disabled."]);

        return RedirectToAction(nameof(Index));
    }

    private JsonObject Validate(JobScheduleEditViewModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.Cron) && !_scheduleManager.IsValidCron(model.Cron.Trim()))
        {
            ModelState.AddModelError(nameof(JobScheduleEditViewModel.Cron), S["The schedule is not a valid cron expression with five fields."]);
        }

        JsonObject parameters = null;

        if (!string.IsNullOrWhiteSpace(model.Parameters) && !JobController.TryParseParameters(model.Parameters, out parameters))
        {
            ModelState.AddModelError(nameof(JobScheduleEditViewModel.Parameters), S["The parameters must be a JSON object, such as {0}.", "{ \"name\": \"value\" }"]);
        }

        return parameters;
    }

    private void Populate(JobScheduleEditViewModel model, JobSchedule schedule)
    {
        model.IsDeclared = schedule?.IsDeclared == true;
        model.DefaultCron = model.IsDeclared ? _displayService.Find(schedule.JobName)?.Schedule : null;

        model.Jobs = schedule is not null
            ? [new SelectListItem(_displayService.GetTitle(schedule.JobName), schedule.JobName)]
            : GetSchedulableJobs();
    }

    private List<SelectListItem> GetSchedulableJobs()
    {
        return _displayService.GetJobs()
            .Where(job => job.CanRunManually)
            .Select(job => new SelectListItem(_displayService.GetTitle(job), job.Name))
            .ToList();
    }
}
