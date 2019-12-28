using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class SitemapIndexController : Controller, IUpdateModel
    {
        private readonly ISitemapHelperService _sitemapService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISitemapIdGenerator _sitemapIdGenerator;
        private readonly ISitemapManager _sitemapManager;
        private readonly ISitemapCacheProvider _sitemapCacheProvider;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;

        public SitemapIndexController(
            ISitemapHelperService sitemapService,
            IAuthorizationService authorizationService,
            ISitemapIdGenerator sitemapIdGenerator,
            ISitemapManager sitemapManager,
            ISitemapCacheProvider sitemapCacheProvider,
            IHtmlLocalizer<AdminController> htmlLocalizer,
            INotifier notifier)
        {
            _sitemapService = sitemapService;
            _authorizationService = authorizationService;
            _sitemapIdGenerator = sitemapIdGenerator;
            _sitemapManager = sitemapManager;
            _sitemapCacheProvider = sitemapCacheProvider;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Edit()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemaps = await _sitemapManager.ListSitemapsAsync();

            var sitemap = sitemaps.FirstOrDefault(s => s.GetType() == typeof(SitemapIndex));

            if (sitemap == null)
            {
                sitemap = new SitemapIndex
                {
                    SitemapId = _sitemapIdGenerator.GenerateUniqueId()
                };
            }

            var indexSource = sitemap.SitemapSources.FirstOrDefault() as SitemapIndexSource;
            var isNew = false;

            if (indexSource == null)
            {
                indexSource = new SitemapIndexSource
                {
                    Id = _sitemapIdGenerator.GenerateUniqueId()
                };
                isNew = true;
            }

            var containableSitemaps = sitemaps
                .Where(s => s.GetType() != typeof(SitemapIndex))
                .Cast<Sitemap>()
                .Select(s => new ContainableSitemapEntryViewModel
                {
                    SitemapId = s.SitemapId,
                    Name = s.Name,
                    IsChecked = indexSource.ContainedSitemapIds.Any(id => id == s.SitemapId)
                })
                .OrderBy(s => s.Name)
                .ToArray();

            var model = new EditSitemapIndexSourceViewModel
            {
                IsNew = isNew,
                SitemapId = sitemap.SitemapId,
                Enabled = sitemap.Enabled,
                Path = sitemap.Path,
                SitemapIndexSource = indexSource,
                ContainableSitemaps = containableSitemaps
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSitemapIndexSourceViewModel model)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Unauthorized();
            }

            var sitemap = (await _sitemapManager.ListSitemapsAsync())
                .FirstOrDefault(s => s.GetType() == typeof(SitemapIndex));

            if (sitemap == null)
            {
                sitemap = new SitemapIndex
                {
                    SitemapId = model.SitemapId
                };
            }
            else
            {
                sitemap = await _sitemapManager.LoadSitemapAsync(sitemap.SitemapId);
            }

            var isNew = false;
            var indexSource = sitemap.SitemapSources.FirstOrDefault() as SitemapIndexSource;
            if (indexSource == null)
            {
                indexSource = new SitemapIndexSource
                {
                    Id = _sitemapIdGenerator.GenerateUniqueId()
                };
                sitemap.SitemapSources.Add(indexSource);
                isNew = true;
            }

            model.SitemapIndexSource = indexSource;

            if (ModelState.IsValid)
            {
                if (isNew)
                {
                    await _sitemapService.ValidatePathAsync(model.Path, this);
                }
                else
                {
                    await _sitemapService.ValidatePathAsync(model.Path, this, sitemap.SitemapId);
                }
            }

            // Path validation may invalidate model state.
            if (ModelState.IsValid)
            {
                sitemap.Enabled = model.Enabled;
                sitemap.Path = model.Path;

                indexSource.ContainedSitemapIds = model.ContainableSitemaps
                    .Where(m => m.IsChecked)
                    .Select(m => m.SitemapId)
                    .ToArray();

                await _sitemapManager.SaveSitemapAsync(sitemap.SitemapId, sitemap);

                if (!isNew)
                {
                    // Always clear sitemap index cache when updated.
                    await _sitemapCacheProvider.ClearSitemapCacheAsync(sitemap.Path);
                };

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
                return Unauthorized();
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

            return RedirectToAction(nameof(Edit));
        }
    }
}
