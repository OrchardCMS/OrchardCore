using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure.Controllers
{
    [Feature("OrchardCore.Media.Cache")]
    [Admin]
    public class MediaCacheController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaFileStoreCache _mediaFileStoreCache;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<MediaCacheController> H;

        public MediaCacheController(
            IAuthorizationService authorizationService,
            IMediaFileStoreCache mediaFileStoreCache,
            INotifier notifier,
            IHtmlLocalizer<MediaCacheController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _mediaFileStoreCache = mediaFileStoreCache;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, MediaCachePermissions.ManageAssetCache))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Purge()
        {
            if (!await _authorizationService.AuthorizeAsync(User, MediaCachePermissions.ManageAssetCache))
            {
                return Unauthorized();
            }

            var hasErrors = await _mediaFileStoreCache.PurgeAsync();
            if (hasErrors)
            {
                _notifier.Error(H["Asset cache purged, with errors."]);
            }
            else
            {
                _notifier.Information(H["Asset cache purged."]);
            }

            return RedirectToAction("Index");
        }

    }
}
