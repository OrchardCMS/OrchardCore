using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;
using Orchard.Contents;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orchard.Demo.Controllers
{
    public class ContentApiController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public ContentApiController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async Task<IActionResult> GetById([FromServices] IContentManager contentManager, int id)
        {
            var contentItem = await contentManager.GetAsync(id);

            if (contentItem == null)
            {
                return NotFound();
            }

            return new ObjectResult(contentItem);
        }

        public async Task<IActionResult> GetAuthorizedById([FromServices] IContentManager contentManager, int id)
        {
            var contentItem = await contentManager.GetAsync(id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
                return Unauthorized();

            if (contentItem == null)
            {
                return NotFound();
            }

            return new ObjectResult(contentItem);
        }
    }
}
