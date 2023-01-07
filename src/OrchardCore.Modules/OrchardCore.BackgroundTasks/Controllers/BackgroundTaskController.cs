using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;

namespace OrchardCore.BackgroundTasks.Controllers
{
    [Admin]
    public class BackgroundTaskController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IBackgroundTask> _backgroundTasks;
        private readonly BackgroundTaskManager _backgroundTaskManager;
        private readonly PagerOptions _pagerOptions;
        private readonly INotifier _notifier;
        private readonly dynamic New;
        private readonly IHtmlLocalizer H;

        public BackgroundTaskController(
            IAuthorizationService authorizationService,
            IEnumerable<IBackgroundTask> backgroundTasks,
            BackgroundTaskManager backgroundTaskManager,
            IShapeFactory shapeFactory,
            IOptions<PagerOptions> pagerOptions,
            IHtmlLocalizer<BackgroundTaskController> htmlLocalizer,
            INotifier notifier)
        {
            _authorizationService = authorizationService;
            _backgroundTasks = backgroundTasks;
            _backgroundTaskManager = backgroundTaskManager;
            _pagerOptions = pagerOptions.Value;
            _notifier = notifier;

            New = shapeFactory;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index(AdminIndexOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Forbid();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            var items = _backgroundTasks.Select(task => new BackgroundTaskEntry() { Settings = GetSettings(task, document) });

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                items = items.Where(x => x.Settings.Title.Contains(options.Search, StringComparison.OrdinalIgnoreCase)
                    || (x.Settings.Description != null && x.Settings.Description.Contains(options.Search, StringComparison.OrdinalIgnoreCase))
                );
            }

            var taskItems = items.ToList();
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());
            var pagerShape = (await New.Pager(pager)).TotalItemCount(taskItems.Count).RouteData(routeData);

            var model = new BackgroundTaskIndexViewModel
            {
                Tasks = taskItems.OrderBy(entry => entry.Settings.Title).Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList(),
                Pager = pagerShape,
                Options = options,
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Index))]
        [FormValueRequired("submit.Filter")]
        public ActionResult IndexFilterPOST(BackgroundTaskIndexViewModel model)
        {
            return RedirectToAction(nameof(Index), new RouteValueDictionary {
                { "Options.Search", model.Options.Search },
            });
        }

        public async Task<IActionResult> Edit(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

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

            var settings = GetSettings(task, document);

            var model = new BackgroundTaskViewModel
            {
                Name = name,
                Title = settings.Title,
                Enable = settings.Enable,
                Schedule = settings.Schedule,
                DefaultSchedule = task?.GetDefaultSettings().Schedule,
                Description = settings.Description,
                LockTimeout = settings.LockTimeout,
                LockExpiration = settings.LockExpiration
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

            if (String.IsNullOrWhiteSpace(model.Name))
            {
                return NotFound();
            }

            var task = _backgroundTasks.GetTaskByName(model.Name);

            if (task == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var document = await _backgroundTaskManager.LoadDocumentAsync();

                var settings = GetSettings(task, document);

                settings.Schedule = model.Schedule?.Trim();
                settings.LockTimeout = model.LockTimeout;
                settings.LockExpiration = model.LockExpiration;

                await _backgroundTaskManager.UpdateAsync(model.Name, settings);

                await _notifier.SuccessAsync(H["The task has been updated."]);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Forbid();
            }

            var document = await _backgroundTaskManager.LoadDocumentAsync();

            if (!document.Settings.ContainsKey(name))
            {
                return NotFound();
            }

            await _backgroundTaskManager.RemoveAsync(name);

            await _notifier.SuccessAsync(H["The task has been deleted."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

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

            var settings = GetSettings(task, document);

            settings.Enable = true;

            await _backgroundTaskManager.UpdateAsync(name, settings);

            await _notifier.SuccessAsync(H["The task has been enabled."]);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                return NotFound();
            }

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

            var settings = GetSettings(task, document);

            settings.Enable = false;

            await _backgroundTaskManager.UpdateAsync(name, settings);

            await _notifier.SuccessAsync(H["The task has been disabled."]);

            return RedirectToAction(nameof(Index));
        }

        private static BackgroundTaskSettings GetSettings(IBackgroundTask task, BackgroundTaskDocument document)
        {
            var name = task.GetTaskName();

            if (document.Settings.ContainsKey(name))
            {
                return document.Settings[name];
            }

            return task.GetDefaultSettings();
        }
    }
}
