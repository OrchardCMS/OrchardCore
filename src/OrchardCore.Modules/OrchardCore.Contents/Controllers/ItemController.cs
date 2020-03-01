using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Contents.Controllers
{
    public class ItemController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUpdateModelAccessor _updateModelAccessor;

        public ItemController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            IUpdateModelAccessor updateModelAccessor)
        {
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
        }

        public async Task<IActionResult> Display(string contentItemId, string jsonPath)
        {
            if (contentItemId == null)
            {
                return NotFound();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            // It represents a contained content item
            if (!string.IsNullOrEmpty(jsonPath))
            {
                var root = contentItem.Content as JObject;
                contentItem = root.SelectToken(jsonPath)?.ToObject<ContentItem>();

                // Permissions are granted or revoked on the container item.
                if (contentItem == null)
                {
                    return NotFound();
                }
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);

            return View(model);
        }

        public async Task<IActionResult> Preview(string contentItemId)
        {
            if (contentItemId == null)
            {
                return NotFound();
            }

            var versionOptions = VersionOptions.Latest;

            var contentItem = await _contentManager.GetAsync(contentItemId, versionOptions);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PreviewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);

            return View(model);
        }
    }
}
