using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Media.Controllers
{
    [Feature("OrchardCore.Media.MediaCache")]
    [Admin]
    public class MediaCacheController : Controller, IUpdateModel
    {
        private readonly IMediaCacheManager _mediaCacheManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<MediaCacheController> H;
        private readonly IDisplayManager<MediaFileCache> _displayManager;
        private readonly IEnumerable<IMediaFileStoreCache> _mediaFileStoreCaches;

        public MediaCacheController(
            IMediaCacheManager mediaCacheManager,
            IAuthorizationService authorizationService,
            INotifier notifier,
            IDisplayManager<MediaFileCache> displayManager,
            IHtmlLocalizer<MediaCacheController> htmlLocalizer,
            IEnumerable<IMediaFileStoreCache> mediaFileStoreCaches
            )
        {
            _mediaCacheManager = mediaCacheManager;
            _authorizationService = authorizationService;
            _notifier = notifier;
            _displayManager = displayManager;
            H = htmlLocalizer;

            _mediaFileStoreCaches = mediaFileStoreCaches;
        }

        public async Task<IActionResult> Index()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaCache))
            {
                return Unauthorized();
            }

            var items = new List<dynamic>();

            foreach (var cache in _mediaFileStoreCaches)
            {
                var displayModel = cache.GetDisplayModel();
                dynamic item = await _displayManager.BuildDisplayAsync(displayModel, this, "SummaryAdmin");
                items.Add( item);
            }

            var model = new MediaCacheViewModel
            {
                Items = items,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Purge(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageMediaCache))
            {
                return Unauthorized();
            }

            var cache = _mediaFileStoreCaches.FirstOrDefault(x => x.GetDisplayModel().GetType().Name == id);

            var hasErrors = false;
            if (cache != null)
            {
                hasErrors = await cache.ClearCacheAsync();
            } else
            {
                hasErrors = true;
            }

            if (hasErrors)
            {
                _notifier.Error(H["Media cache purged, with errors."]);
            }
            else
            {
                _notifier.Information(H[$"Media cache purged for {id}."]);
            }

            return RedirectToAction("Index");
        }

    }
}
