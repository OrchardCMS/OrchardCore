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
    private readonly BackgroundTaskManager _backgroundTaskManager;
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
        _backgroundTaskManager = backgroundTaskManager;
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

        var items = _backgroundTasks.Select(task =>
        {
            var defaultSettings = task.GetDefaultSettings();

            if (document.Settings.TryGetValue(task.GetTaskName(), out var settings))
            {
                return new BackgroundTaskEntry()
                {
                    Name = defaultSettings.Name,
                    Title = defaultSettings.Title,
                    Description = settings.Description,
                    Enable = settings.Enable,
                };
            }

            return new BackgroundTaskEntry()
            {
                Name = defaultSettings.Name,
                Title = defaultSettings.Title,
                Description = defaultSettings.Description,
                Enable = defaultSettings.Enable,
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

        options.Statuses =
        [
            new SelectListItem(S["Enabled"], "enabled"),
            new SelectListItem(S["Disabled"], "disabled")
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

        var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
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

        var document = await _backgroundTaskManager.LoadDocumentAsync();
        if (!document.Settings.TryGetValue(name, out var settings))
        {
            settings = task.GetDefaultSettings();
        }

        settings.Enable = true;

        await _backgroundTaskManager.UpdateAsync(name, settings);

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

        var document = await _backgroundTaskManager.LoadDocumentAsync();
        if (!document.Settings.TryGetValue(name, out var settings))
        {
            settings = task.GetDefaultSettings();
        }

        settings.Enable = false;

        await _backgroundTaskManager.UpdateAsync(name, settings);

        await _notifier.SuccessAsync(H["The task has been disabled."]);

        return RedirectToAction(nameof(Index));
    }
}
