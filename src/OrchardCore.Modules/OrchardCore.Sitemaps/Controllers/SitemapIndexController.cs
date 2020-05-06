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
    public class SitemapIndexController : Controller
    {
        private readonly ISitemapHelperService _sitemapService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;
        private readonly ISitemapManager _sitemapManager;
        private readonly ISitemapCacheProvider _sitemapCacheProvider;
        private readonly ISiteService _siteService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;

        public SitemapIndexController(
            ISitemapHelperService sitemapService,
            IAuthorizationService authorizationService,
            ISitemapIdGenerator sitemapIdGenerator,
            ISitemapManager sitemapManager,
            ISitemapCacheProvider sitemapCacheProvider,
            ISiteService siteService,
            IUpdateModelAccessor updateModelAccessor,
            IShapeFactory shapeFactory,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier)
        {
            _sitemapService = sitemapService;
            _authorizationService = authorizationService;
            _sitemapIdGenerator = sitemapIdGenerator;
            _sitemapManager = sitemapManager;
            _sitemapCacheProvider = sitemapCacheProvider;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;
            _notifier = notifier;
            New = shapeFactory;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> List(SitemapIndexListOptions options, PagerParameters pagerParameters)
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
                options = new SitemapIndexListOptions();
            }

            var sitemaps = (await _sitemapManager.ListSitemapsAsync())
                .OfType<SitemapIndex>();

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

            var model = new ListSitemapIndexViewModel
            {
                SitemapIndexes = results.Select(sm => new SitemapIndexListEntry { SitemapId = sm.SitemapId, Name = sm.Name, Enabled = sm.Enabled }).ToList(),
                Options = options,
                Pager = pagerShape
            };

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ListSitemapIndexViewModel model)
        {
            return RedirectToAction("List", new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemaps = await _sitemapManager.ListSitemapsAsync();

            var containableSitemaps = sitemaps
                .Where(s => s.GetType() != typeof(SitemapIndex))
                .Select(s => new ContainableSitemapEntryViewModel
                {
                    SitemapId = s.SitemapId,
                    Name = s.Name,
                    IsChecked = false
                })
                .OrderBy(s => s.Name)
                .ToArray();

            var model = new CreateSitemapIndexViewModel
            {
                ContainableSitemaps = containableSitemaps
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSitemapIndexViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = new SitemapIndex
            {
                SitemapId = _sitemapIdGenerator.GenerateUniqueId()
            };

            var indexSource = new SitemapIndexSource
            {
                Id = _sitemapIdGenerator.GenerateUniqueId()
            };

            sitemap.SitemapSources.Add(indexSource);

            if (ModelState.IsValid)
            {
                await _sitemapService.ValidatePathAsync(model.Path, _updateModelAccessor.ModelUpdater);

            }

            // Path validation may invalidate model state.
            if (ModelState.IsValid)
            {
                sitemap.Name = model.Name;
                sitemap.Enabled = model.Enabled;
                sitemap.Path = model.Path;

                indexSource.ContainedSitemapIds = model.ContainableSitemaps
                    .Where(m => m.IsChecked)
                    .Select(m => m.SitemapId)
                    .ToArray();

                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                _notifier.Success(H["Sitemap index created successfully"]);

                return View(model);
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

            var sitemaps = await _sitemapManager.ListSitemapsAsync();

            var sitemap = sitemaps.FirstOrDefault(s => s.SitemapId == sitemapId);

            var indexSource = sitemap.SitemapSources.FirstOrDefault() as SitemapIndexSource;

            var containableSitemaps = sitemaps
                .Where(s => s.GetType() != typeof(SitemapIndex))
                .Select(s => new ContainableSitemapEntryViewModel
                {
                    SitemapId = s.SitemapId,
                    Name = s.Name,
                    IsChecked = indexSource.ContainedSitemapIds.Any(id => id == s.SitemapId)
                })
                .OrderBy(s => s.Name)
                .ToArray();

            var model = new EditSitemapIndexViewModel
            {
                SitemapId = sitemap.SitemapId,
                Name = sitemap.Name,
                Enabled = sitemap.Enabled,
                Path = sitemap.Path,
                SitemapIndexSource = indexSource,
                ContainableSitemaps = containableSitemaps
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSitemapIndexViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemap = await _sitemapManager.LoadSitemapAsync(model.SitemapId);

            if (sitemap == null)
            {
                return NotFound();
            }

            var indexSource = sitemap.SitemapSources.FirstOrDefault() as SitemapIndexSource;

            model.SitemapIndexSource = indexSource;

            if (ModelState.IsValid)
            {
                await _sitemapService.ValidatePathAsync(model.Path, _updateModelAccessor.ModelUpdater, sitemap.SitemapId);
            }

            // Path validation may invalidate model state.
            if (ModelState.IsValid)
            {
                sitemap.Name = model.Name;
                sitemap.Enabled = model.Enabled;
                sitemap.Path = model.Path;

                indexSource.ContainedSitemapIds = model.ContainableSitemaps
                    .Where(m => m.IsChecked)
                    .Select(m => m.SitemapId)
                    .ToArray();

                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                // Always clear sitemap index cache when updated.
                await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);

                _notifier.Success(H["Sitemap index updated successfully"]);

                return View(model);
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

            _notifier.Success(H["Sitemap index deleted successfully"]);

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

            _notifier.Success(H["Sitemap index menu toggled successfully"]);

            return RedirectToAction(nameof(List));
        }
    }
}
