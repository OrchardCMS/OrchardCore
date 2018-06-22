using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using System.Threading.Tasks;

namespace OrchardCore.Demo.Controllers
{
    public class ContentApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;

        public ContentApiController(IAuthorizationService authorizationService, IContentManager contentManager)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
        }

        public async Task<IActionResult> GetById(string id)
        {
            var contentItem = await _contentManager.GetAsync(id);

            if (contentItem == null)
            {
                return NotFound();
            }

            return new ObjectResult(contentItem);
        }

        [Authorize]
        public async Task<IActionResult> GetAuthorizedById(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DemoAPIAccess))
            {
                return Unauthorized();
            }

            var contentItem = await _contentManager.GetAsync(id);

            if (!await _authorizationService.AuthorizeAsync(User, OrchardCore.Contents.Permissions.ViewContent, contentItem))
            {
                return Unauthorized();
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            return new ObjectResult(contentItem);
        }

        [Authorize]
        [IgnoreAntiforgeryToken]
        [HttpPost]
        public async Task<IActionResult> AddContent([FromBody]ContentItem contentItem)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DemoAPIAccess))
            {
                return Unauthorized();
            }

            await _contentManager.CreateAsync(contentItem);

            return new ObjectResult(contentItem);
        }
    }
}
