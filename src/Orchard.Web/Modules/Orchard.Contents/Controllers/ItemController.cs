using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using System.Threading.Tasks;

namespace Orchard.Contents.Controllers
{
    public class ItemController : Controller
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

        // /Contents/Item/Display/72
        public async Task<IActionResult> Display(int id, int? version)
        {
            if (version.HasValue)
            {
                return await Preview(id, version);
            }

            var contentItem = await _contentManager.GetAsync(id, VersionOptions.Published);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return new HttpUnauthorizedResult();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem);

            return View(model);
        }

        // /Contents/Item/Preview/72
        // /Contents/Item/Preview/72?version=5
        public async Task<IActionResult> Preview(int id, int? version)
        {
            if (id == 0)
            {
                return HttpNotFound();
            }

            var versionOptions = VersionOptions.Latest;

            if (version != null)
            {
                versionOptions = VersionOptions.Number((int)version);
            }

            var contentItem = await _contentManager.GetAsync(id, versionOptions);

            if (contentItem == null)
            {
                return HttpNotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.PreviewContent, contentItem))
            {
                return new HttpUnauthorizedResult();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem);

            return View(model);
        }
    }
}
