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
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class SetController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISitemapSetService _sitemapSetService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;

        public SetController(
            IAuthorizationService authorizationService,
            ISitemapSetService sitemapSetService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            ISitemapIdGenerator sitemapIdGenerator,
            IStringLocalizer<SetController> stringLocalizer,
            IHtmlLocalizer<SetController> htmlLocalizer,
            ILogger<SetController> logger)
        {
            _authorizationService = authorizationService;
            _sitemapSetService = sitemapSetService;
            _siteService = siteService;
            New = shapeFactory;
            _notifier = notifier;
            _sitemapIdGenerator = sitemapIdGenerator;

            T = stringLocalizer;
            H = htmlLocalizer;
            Logger = logger;
        }

        public IStringLocalizer T { get; set; }
        public IHtmlLocalizer H { get; set; }
        public ILogger Logger { get; set; }
        public dynamic New { get; set; }

        public async Task<IActionResult> List(SitemapSetListOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new SitemapSetListOptions();
            }

            var trees = await _sitemapSetService.GetAsync();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                trees = trees.Where(dp => dp.Name.Contains(options.Search)).ToList();
            }

            var count = trees.Count();

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;
            IEnumerable<Models.SitemapSet> results = new List<Models.SitemapSet>();

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
                Logger.LogError(ex, "Error when retrieving the list of sitemap sets");
                _notifier.Error(H["Error when retrieving the list of sitemap sets"]);
            }


            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new SitemapSetListViewModel
            {
                SitemapSet = results.Select(x => new SitemapSetEntry { SitemapSet = x }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }


        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var model = new SitemapSetCreateViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SitemapSetCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                var tree = new Models.SitemapSet
                {
                    Id = _sitemapIdGenerator.GenerateUniqueId(),
                    Name = model.Name,
                    RootPath = model.RootPath
                };

                await _sitemapSetService.SaveAsync(tree);

                return RedirectToAction(nameof(List));
            }


            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            var model = new SitemapSetEditViewModel
            {
                Id = tree.Id,
                Name = tree.Name,
                RootPath = tree.RootPath
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SitemapSetEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(model.Id);

            if (tree == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                tree.Name = model.Name;
                tree.RootPath = model.RootPath;

                await _sitemapSetService.SaveAsync(tree);

                _notifier.Success(H["Sitemap set updated successfully"]);

                return RedirectToAction(nameof(List));
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                _notifier.Error(H["Can't find the sitemap set."]);
                return RedirectToAction(nameof(List));
            }

            var removed = await _sitemapSetService.DeleteAsync(tree);


            if (removed == 1)
            {
                _notifier.Success(H["Sitemap set deleted successfully"]);
            }
            else
            {
                _notifier.Error(H["Can't delete the sitemap set."]);
            }

            return RedirectToAction(nameof(List));
        }


        [HttpPost]
        public async Task<IActionResult> Toggle(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var tree = await _sitemapSetService.GetByIdAsync(id);

            if (tree == null)
            {
                return NotFound();
            }

            tree.Enabled = !tree.Enabled;

            await _sitemapSetService.SaveAsync(tree);

            _notifier.Success(H["Sitemap set toggled successfully"]);

            return RedirectToAction(nameof(List));
        }

    }
}
