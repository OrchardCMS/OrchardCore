using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;

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
            var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid();
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

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PreviewContent, contentItem))
            {
                return this.ChallengeOrForbid();
            }

            var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);

            return View(model);
        }
    }
}
