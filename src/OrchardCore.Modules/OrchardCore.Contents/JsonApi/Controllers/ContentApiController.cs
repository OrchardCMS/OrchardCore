using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.JsonApi;
using OrchardCore.Modules;
using System.Threading.Tasks;

namespace OrchardCore.Content.JsonApi.Controllers
{
    [RequireFeatures("OrchardCore.Apis.JsonApi")]
    [Route("api")]
    public class ContentApiController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;

        public ContentApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
        }

        [Route("content/{contentItemId}", Name = RouteHelpers.ApiRouteByIdName)]
        public async Task<IActionResult> GetById(string contentItemId)
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

            return new ObjectResult(contentItem);
        }

        [Route("content/version/{contentItemVersionId}", Name = RouteHelpers.ApiRouteByVersionName)]
        public async Task<IActionResult> GetByVersion(string contentItemVersionId)
        {
            var contentItem = await _contentManager.GetVersionAsync(contentItemVersionId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            return new ObjectResult(contentItem);
        }
    }
}
