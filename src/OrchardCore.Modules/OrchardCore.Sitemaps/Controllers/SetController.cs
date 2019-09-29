using System;
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
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class SetController : Controller, IUpdateModel
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISitemapService _sitemapService;
        private readonly ISiteService _siteService;
        private readonly INotifier _notifier;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;
        private readonly ILogger _logger;

        public SetController(
            IAuthorizationService authorizationService,
            ISitemapService sitemapService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            ISitemapIdGenerator sitemapIdGenerator,
            ILogger<SetController> logger,
            IStringLocalizer<SetController> stringLocalizer,
            IHtmlLocalizer<SetController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _sitemapService = sitemapService;
            _siteService = siteService;
            New = shapeFactory;
            _notifier = notifier;
            _sitemapIdGenerator = sitemapIdGenerator;
            _logger = logger;

            T = stringLocalizer;
            H = htmlLocalizer;
        }

        public IStringLocalizer T { get; }
        public IHtmlLocalizer H { get; }
        public dynamic New { get; }

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

            var document = await _sitemapService.LoadSitemapDocumentAsync();

            if (!string.IsNullOrWhiteSpace(options.Search))
            {
                document.SitemapSets = document.SitemapSets.Where(dp => dp.Name.Contains(options.Search)).ToList();
            }

            var count = document.SitemapSets.Count();

            var startIndex = pager.GetStartIndex();
            var pageSize = pager.PageSize;

            try
            {
                document.SitemapSets = document.SitemapSets
                .Skip(startIndex)
                .Take(pageSize)
                .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when retrieving the list of sitemap sets");
                _notifier.Error(H["Error when retrieving the list of sitemap sets"]);
            }


            // Maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);

            var pagerShape = (await New.Pager(pager)).TotalItemCount(count).RouteData(routeData);

            var model = new SitemapSetListViewModel
            {
                SitemapSets = document.SitemapSets.Select(x => new SitemapSetEntry { SitemapSet = x }).ToList(),
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
                var document = await _sitemapService.LoadSitemapDocumentAsync();

                var sitemapSet = new SitemapSet
                {
                    Id = _sitemapIdGenerator.GenerateUniqueId(),
                    Name = model.Name
                };
                document.SitemapSets.Add(sitemapSet);
                _sitemapService.SaveSitemapDocument(document);

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

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            var model = new SitemapSetEditViewModel
            {
                Id = sitemapSet.Id,
                Name = sitemapSet.Name
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

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(model.Id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                sitemapSet.Name = model.Name;
                _sitemapService.SaveSitemapDocument(document);

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

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                _notifier.Error(H["Can't find the sitemap set."]);
                return RedirectToAction(nameof(List));
            }

            var result = document.SitemapSets.Remove(sitemapSet);

            if (result)
            {
                _sitemapService.SaveSitemapDocument(document);
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

            var document = await _sitemapService.LoadSitemapDocumentAsync();
            var sitemapSet = document.GetSitemapSetById(id);

            if (sitemapSet == null)
            {
                return NotFound();
            }

            sitemapSet.Enabled = !sitemapSet.Enabled;

            _sitemapService.SaveSitemapDocument(document);

            _notifier.Success(H["Sitemap set toggled successfully"]);

            return RedirectToAction(nameof(List));
        }
    }
}
