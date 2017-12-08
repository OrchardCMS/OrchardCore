//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using OrchardCore.ContentManagement;
//using OrchardCore.Contents;
//using OrchardCore.Contents.JsonApi;
//using OrchardCore.Modules;
//using System.Threading.Tasks;

//namespace OrchardCore.Content.JsonApi.Controllers
//{
//    [RequireFeatures("OrchardCore.Apis.JsonApi")]
//    [Route("api")]
//    public class ContentTypesApiController : Controller
//    {
//        private readonly IContentManager _contentManager;
//        private readonly IAuthorizationService _authorizationService;

//        public ContentTypesApiController(
//            IContentManager contentManager,
//            IAuthorizationService authorizationService)
//        {
//            _authorizationService = authorizationService;
//            _contentManager = contentManager;
//        }

//        [Route("contenttypes/{contentType}", Name = RouteHelpers.ContentTypes.ApiRouteByNameName)]
//        public async Task<IActionResult> GetById(string contentType)
//        {

//            return new ObjectResult(contentItem);
//        }
//    }
//}
