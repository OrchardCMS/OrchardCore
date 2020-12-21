using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Deployment.Download
{
    [Admin]
    [Feature("OrchardCore.Contents.Deployment.Download")]
    public class DownloadController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;

        public DownloadController(
            IAuthorizationService authorizationService,
            IContentManager contentManager
            )
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
        }

        [HttpGet]
        public async Task<IActionResult> Display(string contentItemId, bool latest = false)
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
            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var model = new DisplayJsonContentItemViewModel
            {
                ContentItem = contentItem,
                ContentItemJson = JObject.FromObject(contentItem).ToString()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Download(string contentItemId, bool latest = false)
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
            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var jItem = JObject.FromObject(contentItem);

            return File(Encoding.UTF8.GetBytes(jItem.ToString()), "application/json", $"{contentItem.ContentItemId}.json");
        }
    }
}
