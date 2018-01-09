using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Indexes;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Lists.JsonApi.Controllers
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

        [Route("contentitems/{contentItemId}/relationships/{contentType}", Name = RouteHelpers.ContentItems.ApiRouteRelationshipByIdAndTypeName)]
        public async Task<IActionResult> GetByIdAndType(string contentItemId, string contentType)
        {
            var contentItems = await _session
                .Query<ContentItem, ContainedPartIndex>(x => x.ListContentItemId == contentItemId)
                .With<ContentItemIndex>(cii => cii.ContentType == contentType)
                .ListAsync();

            //var authorizedContentItems =
            //    await contentItems.FilterByRole(_authorizationService, Permissions.ViewContent, User);

            return new ObjectResult(contentItems);
        }
    }
}
