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
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Environment.Shell;

namespace OrchardCore.BackgroundTasks.Controllers
{
    [Admin]
    public class BackgroundTaskController : Controller, IUpdateModel
    {
        private readonly ShellSettings _shellSettings;
        private readonly IAuthorizationService _authorizationService;
        private readonly IBackgroundTaskStateProvider _backgroundTaskStateProvider;
        private readonly BackgroundTaskManager _backgroundTaskManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        
        public BackgroundTaskController(
            ShellSettings shellSettings,
            IAuthorizationService authorizationService,
            IBackgroundTaskStateProvider backgroundTaskStateProvider,
            BackgroundTaskManager backgroundTaskManager,
            IShapeFactory shapeFactory,
            ISiteService siteService,
            IStringLocalizer<BackgroundTaskController> stringLocalizer,
            IHtmlLocalizer<BackgroundTaskController> htmlLocalizer,
            INotifier notifier)
        {
            _shellSettings = shellSettings;
            _authorizationService = authorizationService;
            _backgroundTaskStateProvider = backgroundTaskStateProvider;
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

            var count = document.Tasks.Count;

            var tasks = document.Tasks.OrderBy(x => x.Key)
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

            var model = new BackgroundTaskIndexViewModel
            {
                Tasks = tasks.Select(kvp => new BackgroundTaskEntry { Name = kvp.Key, Definition = kvp.Value }).ToList(),
                Pager = pagerShape
            };

            return View(model);
        }

        public async Task<IActionResult> State(string name)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageBackgroundTasks))
            {
                return Unauthorized();
            }

            var state = await _backgroundTaskStateProvider.GetStateAsync(_shellSettings.Name, name);

            if (state == BackgroundTaskState.Empty)
            {
                return NotFound();
            }

            var model = new BackgroundTaskStateViewModel
            {
                Name = name,
                State = state
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
    }
}
