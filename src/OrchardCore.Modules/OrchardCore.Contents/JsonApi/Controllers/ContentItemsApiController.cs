using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.JsonApi;
using OrchardCore.Modules;
using System.Linq;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Content.JsonApi.Controllers
{
    [RequireFeatures("OrchardCore.Apis.JsonApi")]
    [Route("api")]
    public class ContentItemsApiController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISession _session;

        public ContentItemsApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            ISession session)
        {
            _contentManager = contentManager;
            _authorizationService = authorizationService;
            _session = session;
        }

        [Route("contentitems/{contentItemId}", Name = RouteHelpers.ContentItems.ApiRouteByIdName)]
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

        [Route("contentitems/version/{contentItemVersionId}", Name = RouteHelpers.ContentItems.ApiRouteByVersionName)]
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

        //[Route("contentitems/contenttype/{contentType}", Name = RouteHelpers.ContentItems.ApiR)]
        //public async Task<IActionResult> GetByType(string contentType)
        //{
        //    var contentItems = await _session
        //        .Query<ContentItem, ContentItemIndex>(x => x.ContentType == contentType)
        //        .ListAsync();

        //    var authorizedContentItems =
        //        await contentItems.FilterByRole(_authorizationService, Permissions.ViewContent, User);

        //    return new ObjectResult(authorizedContentItems);
        //}
    }
}
