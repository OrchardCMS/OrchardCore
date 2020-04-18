using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Deployment.ViewAsJson
{
    [Admin]
    [Feature("OrchardCore.Contents.ViewAsJson")]
    public class ViewAsJsonController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;

        public ViewAsJsonController(
            IAuthorizationService authorizationService,
            IContentManager contentManager
            )
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
        }

        [HttpGet]
        public async Task<IActionResult> Display(string contentItemId, string returnUrl, bool latest = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Deployment.CommonPermissions.Export))
            {
                return Forbid();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, latest == false ? VersionOptions.Published : VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            // Export permission is required as the overriding permission.
            // Requesting EditContent would allow custom permissions to deny access to this content item.
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var jItem = JObject.FromObject(contentItem);
            jItem.Remove(nameof(ContentItem.Id));

            var model = new DisplayJsonContentItemViewModel
            {
                ContentItem = contentItem,
                ContentItemJson = jItem.ToString()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Export(string contentItemId, string returnUrl, bool latest = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Deployment.CommonPermissions.Export))
            {
                return Forbid();
            }

            var contentItem = await _contentManager.GetAsync(contentItemId, latest == false ? VersionOptions.Published : VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            // Export permission is required as the overriding permission.
            // Requesting EditContent would allow custom permissions to deny access to this content item.
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var jItem = JObject.FromObject(contentItem);
            jItem.Remove(nameof(ContentItem.Id));

            return File(Encoding.UTF8.GetBytes(jItem.ToString()), "application/json", $"{contentItem.ContentItemId}.json");
        }
    }
}
