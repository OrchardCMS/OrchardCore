using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Media.Controllers
{
    [Feature("OrchardCore.Media.Cache")]
    [Admin]
    public class MediaCacheController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaFileStoreCache _mediaFileStoreCache;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;

        public MediaCacheController(
            IAuthorizationService authorizationService,
            IServiceProvider serviceProvider,
            INotifier notifier,
            IHtmlLocalizer<MediaCacheController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            // Resolve from service provider as the service will not be registered if configuration is invalid.
            _mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, MediaCachePermissions.ManageAssetCache))
            {
                return Forbid();
            }
            var model = new MediaCacheViewModel
            {
                IsConfigured = _mediaFileStoreCache != null
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Purge()
        {
            if (!await _authorizationService.AuthorizeAsync(User, MediaCachePermissions.ManageAssetCache))
            {
                return Forbid();
            }

            if (_mediaFileStoreCache == null)
            {
                await _notifier.ErrorAsync(H["The asset cache feature is enabled, but a remote media store feature is not enabled, or not configured with appsettings.json."]);
                RedirectToAction(nameof(Index));
            }

            var hasErrors = await _mediaFileStoreCache.PurgeAsync();
            if (hasErrors)
            {
                await _notifier.ErrorAsync(H["Asset cache purged, with errors."]);
            }
            else
            {
                await _notifier.InformationAsync(H["Asset cache purged."]);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
