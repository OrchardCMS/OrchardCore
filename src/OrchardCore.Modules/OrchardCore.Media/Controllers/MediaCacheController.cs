using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;

namespace OrchardCore.Media.Controllers
{
    [Feature("OrchardCore.Media.MediaCache")]
    [Admin]
    public class MediaCacheController : Controller
    {
        private readonly IMediaCacheManager _mediaCacheManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<MediaCacheController> H;

        public MediaCacheController(
            IMediaCacheManager mediaCacheManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IHtmlLocalizer<MediaCacheController> htmlLocalizer
            )
        {
            _mediaCacheManager = mediaCacheManager;
            _authorizationService = authorizationService;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> MediaCache()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaCache))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PurgeMediaCache()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaCache))
            {
                return Unauthorized();
            }

            var hasErrors = await _mediaCacheManager.ClearMediaCacheAsync();
            if (hasErrors)
            {
                _notifier.Error(H["Media cache purged, with errors."]);
            }
            else
            {
                _notifier.Information(H["Media cache purged."]);
            }

            return RedirectToAction("MediaCache");
        }

    }
}
