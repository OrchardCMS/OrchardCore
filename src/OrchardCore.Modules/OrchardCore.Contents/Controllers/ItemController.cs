using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;

namespace OrchardCore.Contents.Controllers
{
    public class ItemController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;

        public ItemController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService
            )
        {
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
        }

        public async Task<IActionResult> Display(string contentItemId, string jsonPath)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            // It represents a contained content item
            if (!string.IsNullOrEmpty(jsonPath))
            {
                var root = contentItem.Content as JObject;
                contentItem = root.SelectToken(jsonPath)?.ToObject<ContentItem>();

                if (contentItem == null)
                {
                    return NotFound();
                }
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

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
                return Unauthorized();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

            return View(model);
        }
    }
}
