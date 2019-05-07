using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.AdminMenu.Controllers
{
    [Admin]
    public class MenuController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAdminMenuService _AdminMenuService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public MenuController(
            IAuthorizationService authorizationService,
            IAdminMenuService AdminMenuService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<MenuController> stringLocalizer,
            IHtmlLocalizer<MenuController> htmlLocalizer,
            ILogger<MenuController> logger)
        {
            _authorizationService = authorizationService;
            _AdminMenuService = AdminMenuService;
            _siteService = siteService;
            New = shapeFactory;
            _notifier = notifier;

            T = stringLocalizer;
            H = htmlLocalizer;
            Logger = logger;
        }

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }
        public ILogger Logger { get; set; }
        public dynamic New { get; set; }

        public async Task<IActionResult> List(AdminMenuListOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new AdminMenuListOptions();
            }

            var trees = await _AdminMenuService.GetAsync();
            
            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                trees = trees.Where(dp => dp.Name.Contains(options.Search)).ToList();
            }

            var count = trees.Count();

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;
            IEnumerable<Models.AdminMenu> results = new List<Models.AdminMenu>();

            //todo: handle the case where there is a deserialization exception on some of the presets.
            // load at least the ones without error. Provide a way to delete the ones on error.
            try
            {
                results = trees
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error when retrieving the list of admin menus");
                _notifier.Error(H["Error when retrieving the list of admin menus"]);
            }


            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new AdminMenuListViewModel
            {
                AdminMenu = results.Select(x => new AdminMenuEntry { AdminMenu = x }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            var model = new AdminMenuCreateViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminMenuCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var tree = new Models.AdminMenu {Name = model.Name};

                await _AdminMenuService.SaveAsync(tree);
                
                return RedirectToAction(nameof(List));
            }


            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            var tree = await _AdminMenuService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var model = new AdminMenuEditViewModel
            {
                Id = tree.Id,
                Name = tree.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminMenuEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            var tree = await _AdminMenuService.GetByIdAsync(model.Id);

            if (tree == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                tree.Name = model.Name;

                await _AdminMenuService.SaveAsync(tree);                

                _notifier.Success(H["Admin menu updated successfully"]);

                return RedirectToAction(nameof(List));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            var tree = await _AdminMenuService.GetByIdAsync(id);

            if (tree == null)
            {
                _notifier.Error(H["Can't find the admin menu."]);
                return RedirectToAction(nameof(List));
            }

            var removed = await _AdminMenuService.DeleteAsync(tree);


            if (removed == 1)
            {
                _notifier.Success(H["Admin menu deleted successfully"]);
            }
            else
            {
                _notifier.Error(H["Can't delete the admin menu."]);
            }

            return RedirectToAction(nameof(List));
        }


        [HttpPost]
        public async Task<IActionResult> Toggle(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminMenu))
            {
                return Unauthorized();
            }

            var tree = await _AdminMenuService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            tree.Enabled = !tree.Enabled;

            await _AdminMenuService.SaveAsync(tree);

            _notifier.Success(H["Admin menu toggled successfully"]);

            return RedirectToAction(nameof(List));
        }

    }
}
