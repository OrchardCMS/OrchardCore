using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.BackgroundTasks.Controllers;

[Admin("BackgroundTasks/{action}/{name?}", "BackgroundTasks{action}")]
public sealed class BackgroundTaskController : Controller
{
    private const string _optionsSearch = $"{nameof(BackgroundTaskIndexViewModel.Options)}.{nameof(AdminIndexOptions.Search)}";
    private const string _optionsStatus = $"{nameof(BackgroundTaskIndexViewModel.Options)}.{nameof(AdminIndexOptions.Status)}";

    private readonly IAuthorizationService _authorizationService;
    private readonly IEnumerable<IBackgroundTask> _backgroundTasks;
    private readonly BackgroundTaskManager _backgroundTaskManager;
    private readonly IBackgroundTaskMonitor _backgroundTaskMonitor;
    private readonly ShellSettings _shellSettings;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public BackgroundTaskController(
        IAuthorizationService authorizationService,
        IEnumerable<IBackgroundTask> backgroundTasks,
        BackgroundTaskManager backgroundTaskManager,
        IEnumerable<IBackgroundTaskMonitor> backgroundTaskMonitors,
        ShellSettings shellSettings,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        IHtmlLocalizer<BackgroundTaskController> htmlLocalizer,
        IStringLocalizer<BackgroundTaskController> stringLocalizer,
        INotifier notifier)
    {
        _authorizationService = authorizationService;
        _backgroundTasks = backgroundTasks;
        _backgroundTaskManager = backgroundTaskManager;

        // The monitor is registered at the host level with the background service, which may not be registered.
        _backgroundTaskMonitor = backgroundTaskMonitors.LastOrDefault();
        _shellSettings = shellSettings;
        _pagerOptions = pagerOptions.Value;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("BackgroundTasks", "BackgroundTasks")]
    public async Task<IActionResult> Index(AdminIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var document = await _backgroundTaskManager.GetDocumentAsync();
        var states = await GetStatesAsync();

        var items = _backgroundTasks.Select(task =>
        {
            var defaultSettings = task.GetDefaultSettings();

            if (!document.Settings.TryGetValue(task.GetTaskName(), out var settings))
            {
                settings = defaultSettings;
            }

            states.TryGetValue(defaultSettings.Name, out var state);

            return new BackgroundTaskEntry()
            {
                Name = defaultSettings.Name,
                Title = defaultSettings.Title,
                Description = settings.Description,
                Enable = settings.Enable,
                Schedule = settings.Schedule,
                State = state,
            };
        });

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            items = items.Where(entry => entry.Title != null && entry.Title.Contains(options.Search, StringComparison.OrdinalIgnoreCase)
                || (entry.Description != null && entry.Description.Contains(options.Search, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (string.Equals(options.Status, "enabled", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(entry => entry.Enable);
        }
        else if (string.Equals(options.Status, "disabled", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(entry => !entry.Enable);
        }
        else if (string.Equals(options.Status, "running", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(entry => entry.State is not null && entry.State.Status != BackgroundTaskStatus.Idle);
        }
        else if (string.Equals(options.Status, "failed", StringComparison.OrdinalIgnoreCase))
        {
            items = items.Where(entry => entry.State?.LastRun?.Result == BackgroundTaskRunResult.Failed);
        }

        options.Statuses =
        [
            new SelectListItem(S["Enabled"], "enabled"),
            new SelectListItem(S["Disabled"], "disabled"),
        ];

        if (_backgroundTaskMonitor is not null)
        {
            options.Statuses.Add(new SelectListItem(S["Running or queued"], "running"));
            options.Statuses.Add(new SelectListItem(S["Last run failed"], "failed"));
        }

        options.BulkActions =
        [
            new SelectListItem(S["Enable"], nameof(BackgroundTaskBulkAction.Enable)),
            new SelectListItem(S["Disable"], nameof(BackgroundTaskBulkAction.Disable)),
        ];

        if (_backgroundTaskMonitor is not null)
        {
            options.BulkActions.Add(new SelectListItem(S["Run now"], nameof(BackgroundTaskBulkAction.Run)));
        }

        var taskItems = items.ToList();
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(_optionsSearch, options.Search);
        }

        if (!string.IsNullOrEmpty(options.Status))
        {
            routeData.Values.TryAdd(_optionsStatus, options.Status);
        }

        var pager = new Pager(pagerParameters, _pagerOptions);
        var pagerShape = await _shapeFactory.PagerAsync(pager, taskItems.Count, routeData);

        var model = new BackgroundTaskIndexViewModel
        {
            Tasks = taskItems.OrderBy(entry => entry.Title).Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList(),
            Pager = pagerShape,
            Options = options,
            IsMonitored = _backgroundTaskMonitor is not null,
        };

        return View(model);
    }

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPOST(BackgroundTaskIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { _optionsSearch, model.Options.Search },
            { _optionsStatus, model.Options.Status },
        });

    [HttpPost, ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> IndexBulkActionPOST(AdminIndexOptions options, IEnumerable<string> taskNames)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        if (taskNames == null || !taskNames.Any())
        {
            await _notifier.WarningAsync(H["Please select one or more tasks."]);

            return RedirectToAction(nameof(Index));
        }

        if (options.BulkAction == BackgroundTaskBulkAction.Run)
        {
            await RunTasksAsync(taskNames);

            return RedirectToAction(nameof(Index));
        }

        var document = await _backgroundTaskManager.LoadDocumentAsync();

        foreach (var name in taskNames)
        {
            var task = _backgroundTasks.GetTaskByName(name);
            if (task == null)
            {
                continue;
            }

            if (!document.Settings.TryGetValue(name, out var settings))
            {
                settings = task.GetDefaultSettings();
            }

            settings.Enable = options.BulkAction switch
            {
                BackgroundTaskBulkAction.Enable => true,
                BackgroundTaskBulkAction.Disable => false,
                _ => settings.Enable,
            };

            await _backgroundTaskManager.UpdateAsync(name, settings);
        }

        switch (options.BulkAction)
        {
            case BackgroundTaskBulkAction.Enable:
                await _notifier.SuccessAsync(H["The tasks have been enabled."]);
                break;
            case BackgroundTaskBulkAction.Disable:
                await _notifier.SuccessAsync(H["The tasks have been disabled."]);
                break;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var task = _backgroundTasks.GetTaskByName(name);
        if (task == null)
        {
            return NotFound();
        }

        var document = await _backgroundTaskManager.GetDocumentAsync();

        var defaultSettings = task.GetDefaultSettings();
        if (!document.Settings.TryGetValue(name, out var settings))
        {
            settings = defaultSettings;
        }

        var model = new BackgroundTaskDetailsViewModel
        {
            Name = defaultSettings.Name,
            Title = defaultSettings.Title,
            Description = settings.Description,
            Enable = settings.Enable,
            Schedule = settings.Schedule,
            DefaultSchedule = defaultSettings.Schedule,
            UsePipeline = settings.UsePipeline,
            LockTimeout = settings.LockTimeout,
            LockExpiration = settings.LockExpiration,
            IsMonitored = _backgroundTaskMonitor is not null,
            State = _backgroundTaskMonitor is not null
                ? await _backgroundTaskMonitor.GetStateAsync(_shellSettings.Name, defaultSettings.Name)
                : null,
        };

        return View(model);
    }

    /// <summary>
    /// Returns the execution status of the tasks, polled by the admin pages while a task is queued or running.
    /// </summary>
    public async Task<IActionResult> States()
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var states = await GetStatesAsync();

        return Json(states.Values.Select(state => new
        {
            state.Name,
            Status = state.Status.ToString(),
            state.LastStartTime,
            state.RunCount,
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Run(string name, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var task = _backgroundTasks.GetTaskByName(name);
        if (task == null)
        {
            return NotFound();
        }

        await RunTasksAsync([name]);

        return RedirectToReturnUrlOrIndex(returnUrl);
    }

    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var task = _backgroundTasks.GetTaskByName(name);
        if (task == null)
        {
            return NotFound();
        }

        var document = await _backgroundTaskManager.GetDocumentAsync();

        var defaultSettings = task.GetDefaultSettings();
        if (!document.Settings.TryGetValue(name, out var settings))
        {
            settings = defaultSettings;
        }

        var model = new BackgroundTaskViewModel
        {
            Name = defaultSettings.Name,
            Title = defaultSettings.Title,
            DefaultSchedule = defaultSettings.Schedule,
            Schedule = settings.Schedule,
            Description = settings.Description,
            LockTimeout = settings.LockTimeout,
            LockExpiration = settings.LockExpiration,
            UsePipeline = settings.UsePipeline,
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BackgroundTaskViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var task = _backgroundTasks.GetTaskByName(model.Name);
        if (task == null)
        {
            return NotFound();
        }

        var defaultSettings = task.GetDefaultSettings();

        if (ModelState.IsValid)
        {
            var document = await _backgroundTaskManager.LoadDocumentAsync();
            if (!document.Settings.TryGetValue(model.Name, out var settings))
            {
                settings = defaultSettings;
            }

            settings.Title = defaultSettings.Title;
            settings.Schedule = model.Schedule?.Trim();
            settings.Description = model.Description;
            settings.LockTimeout = model.LockTimeout;
            settings.LockExpiration = model.LockExpiration;
            settings.UsePipeline = model.UsePipeline;

            await _backgroundTaskManager.UpdateAsync(model.Name, settings);

            await _notifier.SuccessAsync(H["The task has been updated."]);

            return RedirectToAction(nameof(Index));
        }

        model.Title = defaultSettings.Title;
        model.DefaultSchedule = defaultSettings.Schedule;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Enable(string name, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var task = _backgroundTasks.GetTaskByName(name);
        if (task == null)
        {
            return NotFound();
        }

        var document = await _backgroundTaskManager.LoadDocumentAsync();
        if (!document.Settings.TryGetValue(name, out var settings))
        {
            settings = task.GetDefaultSettings();
        }

        settings.Enable = true;

        await _backgroundTaskManager.UpdateAsync(name, settings);

        await _notifier.SuccessAsync(H["The task has been enabled."]);

        return RedirectToReturnUrlOrIndex(returnUrl);
    }

    [HttpPost]
    public async Task<IActionResult> Disable(string name, string returnUrl)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
        {
            return Forbid();
        }

        var task = _backgroundTasks.GetTaskByName(name);
        if (task == null)
        {
            return NotFound();
        }

        var document = await _backgroundTaskManager.LoadDocumentAsync();
        if (!document.Settings.TryGetValue(name, out var settings))
        {
            settings = task.GetDefaultSettings();
        }

        settings.Enable = false;

        await _backgroundTaskManager.UpdateAsync(name, settings);

        await _notifier.SuccessAsync(H["The task has been disabled."]);

        return RedirectToReturnUrlOrIndex(returnUrl);
    }

    private IActionResult RedirectToReturnUrlOrIndex(string returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return this.LocalRedirect(returnUrl, true);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Dictionary<string, BackgroundTaskState>> GetStatesAsync()
    {
        if (_backgroundTaskMonitor is null)
        {
            return [];
        }

        var states = await _backgroundTaskMonitor.GetStatesAsync(_shellSettings.Name);

        return states.ToDictionary(state => state.Name);
    }

    private async Task RunTasksAsync(IEnumerable<string> names)
    {
        if (_backgroundTaskMonitor is null)
        {
            await _notifier.ErrorAsync(H["Background tasks can't be run on demand because the background service is not registered."]);

            return;
        }

        foreach (var name in names)
        {
            var task = _backgroundTasks.GetTaskByName(name);
            if (task == null)
            {
                continue;
            }

            var title = task.GetDefaultSettings().Title;

            switch (await _backgroundTaskMonitor.RequestRunAsync(_shellSettings.Name, name))
            {
                case BackgroundTaskRunRequestResult.Queued:
                    await _notifier.SuccessAsync(H["The task '{0}' has been queued and will run shortly.", title]);
                    break;

                case BackgroundTaskRunRequestResult.AlreadyQueued:
                    await _notifier.InformationAsync(H["The task '{0}' is already queued to run.", title]);
                    break;

                case BackgroundTaskRunRequestResult.AlreadyRunning:
                    await _notifier.WarningAsync(H["The task '{0}' is already running.", title]);
                    break;

                default:
                    await _notifier.WarningAsync(H["The task '{0}' can't run yet because the background service didn't load it. Please try again in a moment.", title]);
                    break;
            }
        }
    }
}
