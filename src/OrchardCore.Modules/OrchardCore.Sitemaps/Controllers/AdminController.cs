using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class AdminController : Controller
    {
        private readonly ISitemapHelperService _sitemapService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<SitemapSource> _displayManager;
        private readonly IEnumerable<ISitemapSourceFactory> _sourceFactories;
        private readonly ISitemapManager _sitemapManager;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;
        private readonly ISiteService _siteService;
        private readonly ISitemapCacheProvider _sitemapCacheProvider;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public AdminController(
            ISitemapHelperService sitemapService,
            IAuthorizationService authorizationService,
            IDisplayManager<SitemapSource> displayManager,
            IEnumerable<ISitemapSourceFactory> sourceFactories,
            ISitemapManager sitemapManager,
            ISitemapIdGenerator sitemapIdGenerator,
            ISiteService siteService,
            ISitemapCacheProvider sitemapCacheProvider,
            IUpdateModelAccessor updateModelAccessor,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _sitemapService = sitemapService;
            _displayManager = displayManager;
            _sourceFactories = sourceFactories;
            _authorizationService = authorizationService;
            _sitemapManager = sitemapManager;
            _sitemapIdGenerator = sitemapIdGenerator;
            _siteService = siteService;
            _sitemapCacheProvider = sitemapCacheProvider;
            _updateModelAccessor = updateModelAccessor;
            _notifier = notifier;
            H = htmlLocalizer;
            New = shapeFactory;
        }

        public async Task<IActionResult> List(SitemapListOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);

            // default options
            if (options == null)
            {
                options = new SitemapListOptions();
            }

            var sitemaps = (await _sitemapManager.ListSitemapsAsync())
                .OfType<Sitemap>();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                sitemaps = sitemaps.Where(smp => smp.Name.Contains(options.Search));
            }

            var count = sitemaps.Count();

            var results = sitemaps
                .Skip(pager.GetStartIndex())
                .Take(pager.PageSize)
                .ToList();

            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new ListSitemapViewModel
            {
                Sitemaps = results.Select(sm => new SitemapListEntry { SitemapId = sm.SitemapId, Name = sm.Name, Enabled = sm.Enabled }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ListSitemapViewModel model)
        {
            return RedirectToAction("List", new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Display(string sitemapId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.GetSitemapAsync(sitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var items = new List<dynamic>();
            foreach (var source in sitemap.SitemapSources)
            {
                dynamic item = await _displayManager.BuildDisplayAsync(source, _updateModelAccessor.ModelUpdater, "SummaryAdmin");
                item.SitemapId = sitemap.SitemapId;
                item.SitemapSource = source;
                items.Add(item);
            }

            var thumbnails = new Dictionary<string, dynamic>();
            foreach (var factory in _sourceFactories)
            {
                var source = factory.Create();
                dynamic thumbnail = await _displayManager.BuildDisplayAsync(source, _updateModelAccessor.ModelUpdater, "Thumbnail");
                thumbnail.SitemapSource = source;
                thumbnail.SitemapSourceType = factory.Name;
                thumbnail.Sitemap = sitemap;
                thumbnails.Add(factory.Name, thumbnail);
            }

            var model = new DisplaySitemapViewModel
            {
                Sitemap = sitemap,
                Items = items,
                Thumbnails = thumbnails,
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var model = new CreateSitemapViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSitemapViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Path))
                {
                    model.Path = _sitemapService.GetSitemapSlug(model.Name);
                }

                await _sitemapService.ValidatePathAsync(model.Path, _updateModelAccessor.ModelUpdater);
            }

            if (ModelState.IsValid)
            {
                var sitemap = new Sitemap
                {
                    SitemapId = _sitemapIdGenerator.GenerateUniqueId(),
                    Name = model.Name,
                    Path = model.Path,
                    Enabled = model.Enabled
                };

                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                return RedirectToAction(nameof(List));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(string sitemapId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = (await _sitemapManager.GetSitemapAsync(sitemapId)) as Sitemap;

            if (sitemap == null)
            {
                return NotFound();
            }

            var model = new EditSitemapViewModel
            {
                SitemapId = sitemap.SitemapId,
                Name = sitemap.Name,
                Enabled = sitemap.Enabled,
                Path = sitemap.Path
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSitemapViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = (await _sitemapManager.LoadSitemapAsync(model.SitemapId)) as Sitemap;

            if (sitemap == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (String.IsNullOrEmpty(model.Path))
                {
                    model.Path = _sitemapService.GetSitemapSlug(model.Name);
                }

                await _sitemapService.ValidatePathAsync(model.Path, _updateModelAccessor.ModelUpdater, model.SitemapId);
            }

            if (ModelState.IsValid)
            {
                sitemap.Name = model.Name;
                sitemap.Enabled = model.Enabled;
                sitemap.Path = model.Path;

                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                _notifier.Success(H["Sitemap updated successfully"]);

                return RedirectToAction(nameof(List));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string sitemapId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.LoadSitemapAsync(sitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            // Clear sitemap cache when deleted.
            await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);

            await _sitemapManager.DeleteSitemapAsync(sitemapId);

            _notifier.Success(H["Sitemap deleted successfully"]);

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(string sitemapId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.LoadSitemapAsync(sitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            sitemap.Enabled = !sitemap.Enabled;

            await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

            await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);

            _notifier.Success(H["Sitemap toggled successfully"]);

            return RedirectToAction(nameof(List));
        }
    }
}
