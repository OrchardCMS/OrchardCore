using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Sitemaps.Cache;
using OrchardCore.Sitemaps.ViewModels;

namespace OrchardCore.Sitemaps.Controllers
{
    [Admin]
    public class SitemapCacheController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ISitemapCacheProvider _sitemapCacheProvider;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;

        public SitemapCacheController(
            IAuthorizationService authorizationService,
            ISitemapCacheProvider sitemapCacheProvider,
            INotifier notifier,
            IHtmlLocalizer<SitemapCacheController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _sitemapCacheProvider = sitemapCacheProvider;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> List()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var model = new ListSitemapCacheViewModel
            {
                CachedFileNames = (await _sitemapCacheProvider.ListAsync()).ToArray()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PurgeAll()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var hasErrors = await _sitemapCacheProvider.PurgeAllAsync();
            if (hasErrors)
            {
                await _notifier.ErrorAsync(H["Sitemap cache purged, with errors."]);
            }
            else
            {
                await _notifier.InformationAsync(H["Sitemap cache purged."]);
            }

            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> Purge(string cacheFileName)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSitemaps))
            {
                return Forbid();
            }

            var failed = await _sitemapCacheProvider.PurgeAsync(cacheFileName);
            if (failed)
            {
                await _notifier.ErrorAsync(H["Error purging sitemap cache item."]);
            }
            else
            {
                await _notifier.InformationAsync(H["Sitemap cache item purged."]);
            }

            return RedirectToAction(nameof(List));
        }
    }
}
