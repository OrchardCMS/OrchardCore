using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;

namespace OrchardCore.Demo.Controllers
{
    [Route("api/demo")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ContentApiController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentManager _contentManager;
        private readonly IStringLocalizer S;

        public ContentApiController(IAuthorizationService authorizationService, IContentManager contentManager, IStringLocalizer<ContentApiController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            S = stringLocalizer;
        }

        [HttpGet]
        [Route("~/api/demo/sayhello")]
        public IActionResult SayHello()
        {
            return Content(S["Hello!"]);
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
