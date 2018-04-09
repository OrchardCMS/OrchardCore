using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Helpers;
using YesSql;

namespace OrchardCore.Content.Controllers
{
    [Route("api")]
    [ApiExplorerSettings(IgnoreApi = false, GroupName = nameof(ApiController))]
    public class ApiController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;

        public ApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            ISession session)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _session = session;
        }

        [Route("contentitems/{contentItemId}", Name = RouteHelpers.ContentItems.ApiRouteByIdName)]
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetById(string contentItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApiViewContent))
            {
                return Unauthorized();
            }

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

        [Route("contentitems/version/{contentItemVersionId}", Name = RouteHelpers.ContentItems.ApiRouteByVersionName)]
        [HttpGet]
        public async Task<IActionResult> GetByVersion(string contentItemVersionId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApiViewContent))
            {
                return Unauthorized();
            }

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

        [Route("contentitems/type/{contentType}", Name = RouteHelpers.ContentItems.ApiRouteByTypeName)]
        [HttpGet]
        public async Task<IActionResult> GetByType(string contentType)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ApiViewContent))
            {
                return Unauthorized();
            }

            var contentItems = await _session
                .Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentType)
                .ListAsync();

            // TODO: Should there be authorization here?

            return new ObjectResult(contentItems);
        }
    }
}
