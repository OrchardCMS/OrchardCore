using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;

namespace OrchardCore.Demo.Controllers
{
    [Route("api/demo")]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    [ApiController]
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

        public async Task<IActionResult> GetAuthorizedById(string id)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DemoAPIAccess))
            {
                return this.ChallengeOrForbid("Api");
            }

            var contentItem = await _contentManager.GetAsync(id);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid("Api");
            }

            if (contentItem == null)
            {
                return NotFound();
            }

            return new ObjectResult(contentItem);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddContent(ContentItem contentItem)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DemoAPIAccess))
            {
                return this.ChallengeOrForbid("Api");
            }

            await _contentManager.CreateAsync(contentItem);

            return new ObjectResult(contentItem);
        }
    }
}
