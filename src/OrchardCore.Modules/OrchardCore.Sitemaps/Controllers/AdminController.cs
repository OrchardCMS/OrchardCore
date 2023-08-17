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
    public class AdminController : Controller
    {
        private readonly ISitemapHelperService _sitemapService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisplayManager<SitemapSource> _displayManager;
        private readonly IEnumerable<ISitemapSourceFactory> _sourceFactories;
        private readonly ISitemapManager _sitemapManager;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;
        private readonly PagerOptions _pagerOptions;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly INotifier _notifier;
        protected readonly IStringLocalizer S;
        protected readonly IHtmlLocalizer H;
        protected readonly dynamic New;

        public AdminController(
            ISitemapHelperService sitemapService,
            IAuthorizationService authorizationService,
            IDisplayManager<SitemapSource> displayManager,
            IEnumerable<ISitemapSourceFactory> sourceFactories,
            ISitemapManager sitemapManager,
            ISitemapIdGenerator sitemapIdGenerator,
            IOptions<PagerOptions> pagerOptions,
            IUpdateModelAccessor updateModelAccessor,
            INotifier notifier,
            IShapeFactory shapeFactory,
            IStringLocalizer<AdminController> stringLocalizer,
            IHtmlLocalizer<AdminController> htmlLocalizer)
        {
            _sitemapService = sitemapService;
            _displayManager = displayManager;
            _sourceFactories = sourceFactories;
            _authorizationService = authorizationService;
            _sitemapManager = sitemapManager;
            _sitemapIdGenerator = sitemapIdGenerator;
            _pagerOptions = pagerOptions.Value;
            _updateModelAccessor = updateModelAccessor;
            _notifier = notifier;
            S = stringLocalizer;
            H = htmlLocalizer;
            New = shapeFactory;
        }

        public async Task<IActionResult> List(ContentOptions options, PagerParameters pagerParameters)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var pager = new Pager(pagerParameters, _pagerOptions.GetPageSize());

            var sitemaps = (await _sitemapManager.GetSitemapsAsync())
                .OfType<Sitemap>();

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

            var model = new ListSitemapViewModel
            {
                Sitemaps = results.Select(sm => new SitemapListEntry { SitemapId = sm.SitemapId, Name = sm.Name, Enabled = sm.Enabled }).ToList(),
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
        public ActionResult ListFilterPOST(ListSitemapViewModel model)
        {
            return RedirectToAction(nameof(List), new RouteValueDictionary {
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

                await _sitemapManager.UpdateSitemapAsync(sitemap);

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

                await _sitemapManager.UpdateSitemapAsync(sitemap);

                await _notifier.SuccessAsync(H["Sitemap updated successfully."]);

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

            await _notifier.SuccessAsync(H["Sitemap deleted successfully."]);

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

            await _notifier.SuccessAsync(H["Sitemap toggled successfully."]);

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
                        await _notifier.SuccessAsync(H["Sitemaps successfully removed."]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
                }
            }

            return RedirectToAction(nameof(List));
        }
    }
}
