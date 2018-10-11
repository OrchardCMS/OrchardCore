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
using OrchardCore.AdminTrees.Indexes;
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
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public TreeController(
            IAuthorizationService authorizationService,
            ISession session,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IStringLocalizer<TreeController> stringLocalizer,
            IHtmlLocalizer<TreeController> htmlLocalizer,
            ILogger<TreeController> logger)
        {
            _authorizationService = authorizationService;
            _session = session;
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

            var trees = _session.Query<AdminTree, AdminTreeIndex>();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                trees = trees.Where(dp => dp.Name.Contains(options.Search));
            }

            var count = await trees.CountAsync();

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;
            IEnumerable<AdminTree> results = new List<AdminTree>();

            //todo: handle the case where there is a deserialization exception on some of the presets.
            // load at least the ones without error. Provide a way to delete the ones on error.
            try
            {
                results = await trees
                .Skip(startIndex)
                .Take(pageSize)
                .ListAsync();
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
                var tree = new AdminTree { Name = model.Name, Enabled = model.Enabled };

                _session.Save(tree);
                return RedirectToAction(nameof(NodeController.List), "Node", new { Id = tree.Id});
            }


            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var tree = await _session.GetAsync<AdminTree>(id);

            if (tree == null)
            {
                return NotFound();
            }

            var model = new AdminTreeEditViewModel
            {
                Id = tree.Id,
                Name = tree.Name,
                Enabled = tree.Enabled            
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

            var tree = await _session.GetAsync<AdminTree>(model.Id);

            if (tree == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrWhiteSpace(model.Name))
                {
                    ModelState.AddModelError(nameof(AdminTreeEditViewModel.Name), T["The name is mandatory."]);
                }
            }

            if (ModelState.IsValid)
            {
                tree.Name = model.Name;
                tree.Enabled = model.Enabled;

                _session.Save(tree);

                _notifier.Success(H["Admin tree updated successfully"]);

                return RedirectToAction(nameof(List));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdminTree))
            {
                return Unauthorized();
            }

            var tree = await _session.GetAsync<AdminTree>(id);

            if (tree == null)
            {
                return NotFound();
            }

            _session.Delete(tree);

            _notifier.Success(H["Admin tree deleted successfully"]);

            return RedirectToAction(nameof(List));
        }
    }
}
