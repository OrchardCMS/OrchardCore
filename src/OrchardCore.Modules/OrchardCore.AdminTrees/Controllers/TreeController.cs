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
using OrchardCore.AdminTrees.Models;
using OrchardCore.AdminTrees.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.AdminTrees.Controllers
{
    [Admin]
    public class TreeController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IAdminTreeService _adminTreeService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public TreeController(
            IAuthorizationService authorizationService,
            IAdminTreeService adminTreeService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<TreeController> stringLocalizer,
            IHtmlLocalizer<TreeController> htmlLocalizer,
            ILogger<TreeController> logger)
        {
            _authorizationService = authorizationService;
            _adminTreeService = adminTreeService;
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

        public async Task<IActionResult> List(AdminTreeListOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new AdminTreeListOptions();
            }

            var trees = await _adminTreeService.GetAsync();
            
            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                trees = trees.Where(dp => dp.Name.Contains(options.Search)).ToList();
            }

            var count = trees.Count();

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;
            IEnumerable<AdminTree> results = new List<AdminTree>();

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
                Logger.LogError(ex, "Error when retrieving the list of admin trees");
                _notifier.Error(H["Error when retrieving the list of admin trees"]);
            }


            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new AdminTreeListViewModel
            {
                AdminTrees = results.Select(x => new AdminTreeEntry { AdminTree = x }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var model = new AdminTreeCreateViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdminTreeCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var tree = new AdminTree {Name = model.Name};

                await _adminTreeService.SaveAsync(tree);
                
                return RedirectToAction(nameof(List));
            }


            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var tree = await _adminTreeService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var model = new AdminTreeEditViewModel
            {
                Id = tree.Id,
                Name = tree.Name
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AdminTreeEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var tree = await _adminTreeService.GetByIdAsync(model.Id);

            if (tree == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                tree.Name = model.Name;

                await _adminTreeService.SaveAsync(tree);                

                _notifier.Success(H["Admin tree updated successfully"]);

                return RedirectToAction(nameof(List));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var tree = await _adminTreeService.GetByIdAsync(id);

            if (tree == null)
            {
                _notifier.Error(H["Can't find the admin tree."]);
                return RedirectToAction(nameof(List));
            }

            var removed = await _adminTreeService.DeleteAsync(tree);


            if (removed == 1)
            {
                _notifier.Success(H["Admin tree deleted successfully"]);
            }
            else
            {
                _notifier.Error(H["Can't delete the admin tree."]);
            }

            return RedirectToAction(nameof(List));
        }


        [HttpPost]
        public async Task<IActionResult> Toggle(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var tree = await _adminTreeService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            tree.Enabled = !tree.Enabled;

            await _adminTreeService.SaveAsync(tree);

            _notifier.Success(H["Admin tree toggled successfully"]);

            return RedirectToAction(nameof(List));
        }

    }
}
