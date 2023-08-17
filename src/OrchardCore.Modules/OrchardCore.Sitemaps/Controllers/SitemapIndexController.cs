using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
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
        private readonly PagerOptions _pagerOptions;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;
        protected readonly dynamic New;

        public SitemapIndexController(
            ISitemapHelperService sitemapService,
            IAuthorizationService authorizationService,
            ISitemapIdGenerator sitemapIdGenerator,
            ISitemapManager sitemapManager,
            IOptions<PagerOptions> pagerOptions,
            IUpdateModelAccessor updateModelAccessor,
            IShapeFactory shapeFactory,
            IStringLocalizer<SitemapIndexController> stringLocalizer,
            IHtmlLocalizer<SitemapIndexController> htmlLocalizer,
            INotifier notifier)
        {
            _sitemapService = sitemapService;
            _authorizationService = authorizationService;
            _sitemapIdGenerator = sitemapIdGenerator;
            _sitemapManager = sitemapManager;
            _pagerOptions = pagerOptions.Value;
            _updateModelAccessor = updateModelAccessor;
            _notifier = notifier;
            New = shapeFactory;
            S = stringLocalizer;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> List(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

            var sitemaps = (await _sitemapManager.GetSitemapsAsync())
                .OfType<SitemapIndex>();

            if (!String.IsNullOrWhiteSpace(options.Search))
            {
                sitemaps = sitemaps.Where(x => x.Name.Contains(options.Search, StringComparison.OrdinalIgnoreCase));
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

            model.Options.ContentsBulkAction = new List<SelectListItem>() {
                new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) }
            };

            return View(model);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.Filter")]
        public ActionResult ListFilterPOST(ListSitemapIndexViewModel model)
        {
            return RedirectToAction(nameof(List), new RouteValueDictionary {
                { "Options.Search", model.Options.Search }
            });
        }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var sitemaps = await _sitemapManager.GetSitemapsAsync();

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

                await _sitemapManager.UpdateSitemapAsync(sitemap);

                await _notifier.SuccessAsync(H["Sitemap index created successfully"]);

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

            var sitemap = (await _sitemapManager.GetSitemapAsync(sitemapId)) as SitemapIndex;

            if (sitemap == null)
            {
                return NotFound();
            }

            var sitemaps = await _sitemapManager.GetSitemapsAsync();

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

                await _sitemapManager.UpdateSitemapAsync(sitemap);

                await _notifier.SuccessAsync(H["Sitemap index updated successfully"]);

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

            await _sitemapManager.DeleteSitemapAsync(sitemapId);

            await _notifier.SuccessAsync(H["Sitemap index deleted successfully."]);

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

            await _sitemapManager.UpdateSitemapAsync(sitemap);

            await _notifier.SuccessAsync(H["Sitemap index menu toggled successfully."]);

            return RedirectToAction(nameof(List));
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("submit.BulkAction")]
        public async Task<ActionResult> ListPost(ViewModels.ContentOptions options, IEnumerable<string> itemIds)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            if (itemIds?.Count() > 0)
            {
                var sitemapsList = await _sitemapManager.LoadSitemapsAsync();
                var checkedContentItems = sitemapsList.Where(x => itemIds.Contains(x.SitemapId));
                switch (options.BulkAction)
                {
                    case ContentsBulkAction.None:
                        break;
                    case ContentsBulkAction.Remove:
                        foreach (var item in checkedContentItems)
                        {
                            await _sitemapManager.DeleteSitemapAsync(item.SitemapId);
                        }
                        await _notifier.SuccessAsync(H["Sitemap indices successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }

            return RedirectToAction(nameof(List));
        }
    }
}
