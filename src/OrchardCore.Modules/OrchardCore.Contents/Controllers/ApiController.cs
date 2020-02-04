using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;

namespace OrchardCore.Content.Controllers
{
    [Route("api/content")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
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

        [Route("{contentItemId}"), HttpGet]
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

            return Ok(contentItem);
        }

        [HttpDelete]
        [Route("{contentItemId}")]
        public async Task<IActionResult> Delete(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return StatusCode(204);
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.DeleteContent, contentItem))
            {
                return Unauthorized();
            }

            await _contentManager.RemoveAsync(contentItem);

            return Ok(contentItem);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ContentItem model, bool draft = false)
        {
            // Call LoadedAsync and LoadingAsync only if ContentItem is found
            var contentItem = await _contentManager.GetAsync(model.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.PublishContent))
                {
                    return Unauthorized();
                }

                // Call ActivatedAsync and InitializedAsync
                var newContentItem = await _contentManager.NewAsync(model.ContentType);
                newContentItem.Apply(model);

                // Call UpdatedAsync and UpdatingAsync and CreatedAsync and CreatingAsync
                // In the same order than we do in the AdminController of OrchardCore.Contents.
                await _contentManager.UpdateAndCreateAsync(model, draft ? VersionOptions.DraftRequired : VersionOptions.Published);

                contentItem = newContentItem;
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
                {
                    return Unauthorized();
                }

                contentItem.DisplayText = model.DisplayText;
                contentItem.ModifiedUtc = model.ModifiedUtc;
                contentItem.PublishedUtc = model.PublishedUtc;
                contentItem.CreatedUtc = model.CreatedUtc;
                contentItem.Owner = model.Owner;
                contentItem.Author = model.Author;

                contentItem.Apply(model);
                await _contentManager.UpdateAsync(contentItem);

                if (!draft)
                {
                    await _contentManager.PublishAsync(contentItem); 
                }
            }

            return Ok(contentItem);
        }
    }
}
