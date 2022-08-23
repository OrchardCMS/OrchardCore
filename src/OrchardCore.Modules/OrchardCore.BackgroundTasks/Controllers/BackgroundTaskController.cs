using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Navigation;

namespace OrchardCore.BackgroundTasks.Controllers
{
    [Admin]
    public class BackgroundTaskController : Controller
    {
        private readonly string _tenant;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEnumerable<IBackgroundTask> _backgroundTasks;
        private readonly BackgroundTaskManager _backgroundTaskManager;
        private readonly PagerOptions _pagerOptions;
        private readonly IStringLocalizer S;
        private readonly dynamic New;

        public BackgroundTaskController(
            ShellSettings shellSettings,
            IAuthorizationService authorizationService,
            IEnumerable<IBackgroundTask> backgroundTasks,
            BackgroundTaskManager backgroundTaskManager,
            IShapeFactory shapeFactory,
            IOptions<PagerOptions> pagerOptions,
            IStringLocalizer<BackgroundTaskController> stringLocalizer)
        {
            _tenant = shellSettings.Name;
            _authorizationService = authorizationService;
            _backgroundTasks = backgroundTasks;
            _backgroundTaskManager = backgroundTaskManager;
            _pagerOptions = pagerOptions.Value;

            New = shapeFactory;
            S = stringLocalizer;
        }

        public async Task<IActionResult> Index(PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.PageSize);
            var document = await _backgroundTaskManager.GetDocumentAsync();

            var taskEntries = _backgroundTasks.Select(t =>
            {
                if (!document.Settings.TryGetValue(t.GetTaskName(), out var settings))
                {
                    settings = t.GetDefaultSettings();
                }

                return new BackgroundTaskEntry() { Settings = settings };
            })
            .OrderBy(entry => entry.Settings.Name)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

            var pagerShape = (await New.Pager(pager)).TotalItemCount(_backgroundTasks.Count());

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
                return Forbid();
            }

            var model = new BackgroundTaskViewModel() { Name = name };

            var task = _backgroundTasks.GetTaskByName(name);

            if (task != null)
            {
                var settings = task.GetDefaultSettings();

                model.Enable = settings.Enable;
                model.Schedule = settings.Schedule;
                model.DefaultSchedule = settings.Schedule;
                model.Description = settings.Description;
                model.LockTimeout = settings.LockTimeout;
                model.LockExpiration = settings.LockExpiration;
            }

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost(BackgroundTaskViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(BackgroundTaskViewModel.Name), S["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var settings = new BackgroundTaskSettings
                {
                    Name = model.Name,
                    Enable = model.Enable,
                    Schedule = model.Schedule?.Trim(),
                    Description = model.Description,
                    LockTimeout = model.LockTimeout,
                    LockExpiration = model.LockExpiration
                };

                await _backgroundTaskManager.UpdateAsync(model.Name, settings);

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Forbid();
            }

            var document = await _backgroundTaskManager.GetDocumentAsync();

            if (!document.Settings.ContainsKey(name))
            {
                return RedirectToAction(nameof(Create), new { name });
            }

            var task = _backgroundTasks.GetTaskByName(name);

            var settings = document.Settings[name];

            var model = new BackgroundTaskViewModel
            {
                Name = name,
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

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(BackgroundTaskViewModel.Name), S["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                var settings = new BackgroundTaskSettings
                {
                    Name = model.Name,
                    Enable = model.Enable,
                    Schedule = model.Schedule?.Trim(),
                    Description = model.Description,
                    LockTimeout = model.LockTimeout,
                    LockExpiration = model.LockExpiration
                };

                await _backgroundTaskManager.UpdateAsync(model.Name, settings);

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
                return Forbid();
            }

            var document = await _backgroundTaskManager.LoadDocumentAsync();

            if (!document.Settings.ContainsKey(name))
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
                return Forbid();
            }

            var document = await _backgroundTaskManager.LoadDocumentAsync();

            if (!document.Settings.TryGetValue(name, out var settings))
            {
                settings = _backgroundTasks.GetTaskByName(name)?.GetDefaultSettings();
            }

            if (settings != null)
            {
                settings.Enable = true;
                await _backgroundTaskManager.UpdateAsync(name, settings);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Disable(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Forbid();
            }

            var document = await _backgroundTaskManager.LoadDocumentAsync();

            if (!document.Settings.TryGetValue(name, out var settings))
            {
                settings = _backgroundTasks.GetTaskByName(name)?.GetDefaultSettings();
            }

            if (settings != null)
            {
                settings.Enable = false;
                await _backgroundTaskManager.UpdateAsync(name, settings);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
