using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<Sitemap> _displayManager;
        private readonly IEnumerable<ISitemapProviderFactory> _factories;
        private readonly ISitemapManager _sitemapManager;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;

        public AdminController(
            IAuthorizationService authorizationService,
            IDisplayManager<Sitemap> displayManager,
            IEnumerable<ISitemapProviderFactory> factories,
            ISitemapManager sitemapService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier)
        {
            _authorizationService = authorizationService;
            _displayManager = displayManager;
            _factories = factories;
            _sitemapManager = sitemapService;
            _siteService = siteService;
            New = shapeFactory;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public IHtmlLocalizer H { get; }
        public dynamic New { get; set; }

        public async Task<IActionResult> List(SitemapListOptions options, PagerParameters pagerParameters)
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
                options = new SitemapListOptions();
            }

            var sitemaps = await _sitemapManager.ListSitemapsAsync();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                sitemaps = sitemaps.Where(q => q.Name.IndexOf(options.Search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var results = sitemaps
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(sitemaps.Count()).RouteData(routeData);

            // Build create thumbnails.
            var thumbnails = new Dictionary<string, dynamic>();
            foreach (var factory in _factories)
            {
                var sitemap = factory.Create();
                dynamic thumbnail = await _displayManager.BuildDisplayAsync(sitemap, this, "Thumbnail");
                thumbnail.Sitemap = sitemap;
                thumbnails.Add(factory.Type, thumbnail);
            }

            var model = new SitemapListViewModel
            {
                Thumbnails = thumbnails,
                Sitemaps = new List<dynamic>(),
                Pager = pagerShape
            };

            // Build SummaryAdmin for existing sitemaps.
            foreach (var sitemap in results)
            {
                var shape = (dynamic)await _displayManager.BuildDisplayAsync(sitemap, this, "SummaryAdmin");
                shape.Sitemap = sitemap;
                model.Sitemaps.Add(shape);
            }

            return View(model);
        }

        public async Task<IActionResult> Create(string type)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = _factories.FirstOrDefault(x => x.Type == type)?.Create();

            if (sitemap == null)
            {
                return NotFound();
            }

            var model = new SitemapCreateViewModel
            {
                Editor = await _displayManager.BuildEditorAsync(sitemap, updater: this, isNew: true),
                SitemapType = type
            };

            return View(model);
        }

        [HttpPost, ActionName(nameof(Create))]
        public async Task<IActionResult> CreatePost(SitemapCreateViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = _factories.FirstOrDefault(x => x.Type == model.SitemapType)?.Create();

            if (sitemap == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(sitemap, updater: this, isNew: true);

            if (ModelState.IsValid)
            {
                await _sitemapManager.SaveSitemapAsync(sitemap.Id, sitemap);

                _notifier.Success(H["Sitemap created successfully"]);
                return RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form
            model.Editor = editor;

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = await _sitemapManager.GetSitemapAsync(id);

            if (sitemap == null)
            {
                return NotFound();
            }

            var model = new SitemapEditViewModel
            {
                Id = sitemap.Id,
                Editor = await _displayManager.BuildEditorAsync(sitemap, updater: this, isNew: false)
            };

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public async Task<IActionResult> EditPost(SitemapEditViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = (await _sitemapManager.GetSitemapAsync(model.Id)).Clone();

            if (sitemap == null)
            {
                return NotFound();
            }

            var editor = await _displayManager.UpdateEditorAsync(sitemap, updater: this, isNew: false);

            if (ModelState.IsValid)
            {
                await _sitemapManager.SaveSitemapAsync(model.Id, sitemap);

                _notifier.Success(H["Sitemap updated successfully"]);
                return RedirectToAction("List");
            }

            model.Editor = editor;

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = await _sitemapManager.GetSitemapAsync(id);

            if (sitemap == null)
            {
                return NotFound();
            }

            await _sitemapManager.DeleteSitemapAsync(id);

            _notifier.Success(H["Sitemap deleted successfully"]);

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = await _sitemapManager.GetSitemapAsync(id);

            if (sitemap == null)
            {
                return NotFound();
            }

            sitemap.Enabled = !sitemap.Enabled;

            await _sitemapManager.SaveSitemapAsync(id, sitemap);

            if (sitemap.Enabled)
            {
                _notifier.Success(H["Sitemap enabled successfully"]);
            } else
            {
                _notifier.Success(H["Sitemap disabled successfully"]);
            }
            return RedirectToAction(nameof(List));
        }
    }
}

