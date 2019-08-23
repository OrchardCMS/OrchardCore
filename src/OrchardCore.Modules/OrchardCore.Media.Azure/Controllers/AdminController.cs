using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure.Controllers
{
    [Feature("OrchardCore.Media.Azure.Storage")]
    public class AdminController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IMediaFileStoreCache _mediaFileStoreCache;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<AdminController> H;

        public AdminController(
            IAuthorizationService authorizationService,
            IMediaFileStoreCache mediaFileStoreCache,
            INotifier notifier,
            IHtmlLocalizer<AdminController> htmlLocalizer
            )
        {
            _authorizationService = authorizationService;
            _mediaFileStoreCache = mediaFileStoreCache;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAzureMediaCache))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Purge()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAzureMediaCache))
            {
                return Unauthorized();
            }

            var hasErrors = await _mediaFileStoreCache.PurgeAsync();
            if (hasErrors)
            {
                _notifier.Error(H["Azure media storage cache purged, with errors."]);
            }
            else
            {
                _notifier.Information(H["Azure media storage cache purged."]);
            }

            return RedirectToAction("Index");
        }

    }
}
