using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using System.Threading.Tasks;

namespace OrchardCore.Content.Controllers
{
    public class ApiController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;

        public ApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
        }

        public async Task<IActionResult> Get(string contentItemId)
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
    }
}
