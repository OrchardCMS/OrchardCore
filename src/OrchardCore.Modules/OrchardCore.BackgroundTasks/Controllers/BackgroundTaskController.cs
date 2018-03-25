using System;
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
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.BackgroundTasks.Controllers
{
    [Admin]
    public class BackgroundTaskController : Controller, IUpdateModel
    {
        private readonly string _tenant;
        private readonly IAuthorizationService _authorizationService;
        private readonly IModularBackgroundService _backgroundService;
        private readonly BackgroundTaskManager _backgroundTaskManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        
        public BackgroundTaskController(
            ShellSettings shellSettings,
            IAuthorizationService authorizationService,
            IModularBackgroundService backgroundService,
            BackgroundTaskManager backgroundTaskManager,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<BackgroundTaskController> stringLocalizer,
            IHtmlLocalizer<BackgroundTaskController> htmlLocalizer,
            INotifier notifier)
        {
            _tenant = shellSettings.Name;
            _authorizationService = authorizationService;
            _backgroundService = backgroundService;
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

            var document = await _backgroundTaskManager.GetDocumentAsync();

            var definitions = document.Tasks.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize);

            var settings = await _backgroundService.GetSettingsAsync(_tenant);
            var states = await _backgroundService.GetStatesAsync(_tenant);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(document.Tasks.Count);

            var model = new BackgroundTaskIndexViewModel
            {
                IsRunning = _backgroundService.IsRunning,

                Tasks = definitions.Select(kvp => new BackgroundTaskEntry
                {
                    Name = kvp.Key,
                    Definition = kvp.Value,
                    Settings = settings.FirstOrDefault(s => kvp.Key == s.Name) ?? BackgroundTaskSettings.None,
                    State = states.FirstOrDefault(s => kvp.Key == s.Name) ?? BackgroundTaskState.Undefined
                })
                .ToList(),

                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> Create(BackgroundTaskViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            return View(new BackgroundTaskViewModel() { Names = _backgroundTaskManager.Names });
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
                var definition = new BackgroundTaskDefinition
                {
                    Name = model.Name,
                    Enable = model.Enable,
                    Schedule = model.Schedule?.Trim(),
                    Description = model.Description
                };

                await _backgroundTaskManager.UpdateAsync(model.Name, definition);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            model.Names = _backgroundTaskManager.Names;
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

            var definition = document.Tasks[name];

            var model = new BackgroundTaskViewModel
            {
                Name = name,
                Enable = definition.Enable,
                Schedule = definition.Schedule,
                Description = definition.Description,
                Names = _backgroundTaskManager.Names
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string sourceName, BackgroundTaskViewModel model)
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

            if (!document.Tasks.ContainsKey(sourceName))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var definition = new BackgroundTaskDefinition
                {
                    Name = model.Name,
                    Enable = model.Enable,
                    Schedule = model.Schedule?.Trim(),
                    Description = model.Description
                };

                await _backgroundTaskManager.RemoveAsync(sourceName);
                await _backgroundTaskManager.UpdateAsync(model.Name, definition);

                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            model.Names = _backgroundTaskManager.Names;
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

            _notifier.Success(H["Task Options deleted successfully"]);
            
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Lock(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            _backgroundService.Command(_tenant, name, BackgroundTaskScheduler.CommandCode.Lock);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Unlock(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            _backgroundService.Command(_tenant, name, BackgroundTaskScheduler.CommandCode.Unlock);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ResetCount(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            _backgroundService.Command(_tenant, name, BackgroundTaskScheduler.CommandCode.ResetCount);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ResetFault(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            _backgroundService.Command(_tenant, name, BackgroundTaskScheduler.CommandCode.ResetFault);

            return RedirectToAction(nameof(Index));
        }
    }
}
