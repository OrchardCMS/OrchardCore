using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Models;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.BackgroundTasks.Controllers
{
    [Admin]
    public class BackgroundTaskController : Controller, IUpdateModel
    {
        private readonly string _tenant;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IBackgroundTask> _backgroundTasks;
        private readonly IEnumerable<IBackgroundTaskSettingsProvider> _settingsProviders;
        private readonly BackgroundTaskManager _backgroundTaskManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        
        public BackgroundTaskController(
            ShellSettings shellSettings,
            IAuthorizationService authorizationService,
            IEnumerable<IBackgroundTask> backgroundTasks,
            IEnumerable<IBackgroundTaskSettingsProvider> settingsProviders,
            BackgroundTaskManager backgroundTaskManager,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<BackgroundTaskController> stringLocalizer,
            IHtmlLocalizer<BackgroundTaskController> htmlLocalizer,
            INotifier notifier)
        {
            _tenant = shellSettings.Name;
            _authorizationService = authorizationService;
            _backgroundTasks = backgroundTasks;
            _settingsProviders = settingsProviders;
            _backgroundTaskManager = backgroundTaskManager;
            New = shapeFactory;
            _siteService = siteService;
            _notifier = notifier;
            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public dynamic New { get; set; }

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }

        public async Task<IActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            var taskTypes = _backgroundTasks.Select(t => t.GetType()).ToArray();
            var settings = await _settingsProviders.GetSettingsAsync(taskTypes);

            var taskEntries = settings.Select(s => new BackgroundTaskEntry
            {
                Settings = s,
            })
            .OrderBy(entry => entry.Settings.Name)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

            var pagerShape = (await New.Pager(pager)).TotalItemCount(taskEntries.Count);

            var model = new BackgroundTaskIndexViewModel
            {
                Tasks = taskEntries,
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Create(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var model = new BackgroundTaskViewModel() { Name = name };

            var task = _backgroundTasks.LastOrDefault(t => t.GetType().FullName == name);

            if (task != null)
            {
                var settings = await _settingsProviders.GetSettingsAsync(task.GetType());

                model.Enable = settings.Enable;
                model.Schedule = settings.Schedule;
                model.DefaultSchedule = settings.Schedule;
                model.Description = settings.Description;
            }

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(BackgroundTaskViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(BackgroundTaskViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var task = new BackgroundTask
                {
                    Name = model.Name,
                    Enable = model.Enable,
                    Schedule = model.Schedule?.Trim(),
                    DefaultSchedule = model.DefaultSchedule?.Trim(),
                    Description = model.Description
                };

                await _backgroundTaskManager.UpdateAsync(model.Name, task);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (!document.Tasks.ContainsKey(name))
            {
                return RedirectToAction("Create", new { name });
            }

            var task = document.Tasks[name];

            var model = new BackgroundTaskViewModel
            {
                Name = name,
                Enable = task.Enable,
                Schedule = task.Schedule,
                DefaultSchedule = task.DefaultSchedule,
                Description = task.Description
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BackgroundTaskViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(BackgroundTaskViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var task = new BackgroundTask
                {
                    Name = model.Name,
                    Enable = model.Enable,
                    Schedule = model.Schedule?.Trim(),
                    DefaultSchedule = model.DefaultSchedule?.Trim(),
                    Description = model.Description
                };

                await _backgroundTaskManager.UpdateAsync(model.Name, task);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (!document.Tasks.ContainsKey(name))
            {
                return NotFound();
            }

            await _backgroundTaskManager.RemoveAsync(name);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Enable(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (!document.Tasks.TryGetValue(name, out var task))
            {
                task = new BackgroundTask();
            }

            task.Enable = true;

            await _backgroundTaskManager.UpdateAsync(name, task);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (!document.Tasks.TryGetValue(name, out var task))
            {
                task = new BackgroundTask();
            }

            task.Enable = false;

            await _backgroundTaskManager.UpdateAsync(name, task);

            return RedirectToAction(nameof(Index));
        }
    }
}
