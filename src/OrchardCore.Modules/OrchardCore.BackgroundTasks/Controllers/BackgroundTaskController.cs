using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
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
    private readonly BackgroundTaskManagementService _management;
    private readonly PagerOptions _pagerOptions;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public BackgroundTaskController(
        IAuthorizationService authorizationService,
        IEnumerable<IBackgroundTask> backgroundTasks,
        BackgroundTaskManager backgroundTaskManager,
        IOptions<PagerOptions> pagerOptions,
        IShapeFactory shapeFactory,
        IHtmlLocalizer<BackgroundTaskController> htmlLocalizer,
        IStringLocalizer<BackgroundTaskController> stringLocalizer,
        INotifier notifier)
    {
        _authorizationService = authorizationService;
        _backgroundTasks = backgroundTasks;
        _management = new BackgroundTaskManagementService(backgroundTasks, backgroundTaskManager);
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

        var settings = await _management.ListAsync();
        IEnumerable<BackgroundTaskEntry> items = settings.Select(task => new BackgroundTaskEntry
        {
            Name = task.Name, Title = task.Title, Description = task.Description, Enable = task.Enable,
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

        options.Statuses =
        [
            new SelectListItem(S["Enabled"], "enabled"),
            new SelectListItem(S["Disabled"], "disabled")
        ];

        options.BulkActions =
        [
            new SelectListItem(S["Enable"], nameof(BackgroundTaskBulkAction.Enable)),
            new SelectListItem(S["Disable"], nameof(BackgroundTaskBulkAction.Disable)),
        ];

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

        var invalidSettings = false;
        if (options.BulkAction is BackgroundTaskBulkAction.Enable or BackgroundTaskBulkAction.Disable)
        {
            foreach (var name in taskNames.Distinct(StringComparer.Ordinal))
            {
                var result = await _management.SetStatusAsync(name, options.BulkAction == BackgroundTaskBulkAction.Enable);
                if (result.Errors.Count > 0)
                {
                    invalidSettings = true;
                }
            }
        }

        if (invalidSettings)
        {
            await _notifier.WarningAsync(H["Some tasks have invalid settings and could not be enabled."]);
            return RedirectToAction(nameof(Index));
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

        var defaultSettings = task.GetDefaultSettings();
        var settings = await _management.GetAsync(name);

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

        var input = new BackgroundTaskConfiguration
        {
            Schedule = model.Schedule, Description = model.Description, LockTimeout = model.LockTimeout,
            LockExpiration = model.LockExpiration, UsePipeline = model.UsePipeline,
        };
        foreach (var error in BackgroundTaskManagementService.Validate(input))
        {
            ModelState.AddModelError(error.Key, error.Key switch
            {
                "schedule" => S["Provide a valid five-field cron expression."],
                "lockTimeout" => S["Lock timeout must be nonnegative."],
                "lockExpiration" => S["Lock expiration must be nonnegative."],
                _ => S["The task setting is invalid."],
            });
        }
        if (ModelState.IsValid)
        {
            await _management.UpdateAsync(model.Name, input);
            await _notifier.SuccessAsync(H["The task has been updated."]);
            return RedirectToAction(nameof(Index));
        }

        model.Title = defaultSettings.Title;
        model.DefaultSchedule = defaultSettings.Schedule;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Enable(string name)
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

        var result = await _management.SetStatusAsync(name, true);
        if (result.Errors.Count > 0)
        {
            await _notifier.WarningAsync(H["The task has invalid settings and could not be enabled."]);
            return RedirectToAction(nameof(Edit), new { name });
        }

        await _notifier.SuccessAsync(H["The task has been enabled."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Disable(string name)
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

        var result = await _management.SetStatusAsync(name, false);
        if (result.Errors.Count > 0)
        {
            await _notifier.WarningAsync(H["The task has invalid settings and could not be enabled."]);
            return RedirectToAction(nameof(Edit), new { name });
        }

        await _notifier.SuccessAsync(H["The task has been disabled."]);

        return RedirectToAction(nameof(Index));
    }
}
