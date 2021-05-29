using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Contents.Controllers
{
    [Route("api/content")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
    {
        private static readonly JsonMergeSettings UpdateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };

        private readonly IContentManager _contentManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer S;

        public ApiController(
            IContentManager contentManager,
            IAuthorizationService authorizationService,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _authorizationService = authorizationService;
            _contentManager = contentManager;
            S = stringLocalizer;
        }

        [Route("{contentItemId}"), HttpGet]
        public async Task<IActionResult> Get(string contentItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessContentApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
            {
                return this.ChallengeOrForbid("Api");
            }

            return Ok(contentItem);
        }

        [HttpDelete]
        [Route("{contentItemId}")]
        public async Task<IActionResult> Delete(string contentItemId)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessContentApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return StatusCode(204);
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.DeleteContent, contentItem))
            {
                return this.ChallengeOrForbid("Api");
            }

            await _contentManager.RemoveAsync(contentItem);

            return Ok(contentItem);
        }

        [HttpPost]
        public async Task<IActionResult> Post(ContentItem model, bool draft = false)
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.AccessContentApi))
            {
                return this.ChallengeOrForbid("Api");
            }

            // It is really important to keep the proper method calls order with the ContentManager
            // so that all event handlers gets triggered in the right sequence.

            var contentItem = await _contentManager.GetAsync(model.ContentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent))
                {
                    return this.ChallengeOrForbid("Api");
                }

                var newContentItem = await _contentManager.NewAsync(model.ContentType);
                newContentItem.Merge(model);

                var result = await _contentManager.UpdateValidateAndCreateAsync(newContentItem, VersionOptions.Draft);

                if (!result.Succeeded)
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
                // We check the model state after calling all handlers because they trigger WF content events so, even they are not
                // intended to add model errors (only drivers), a WF content task may be executed inline and add some model errors.
                else if (!ModelState.IsValid)
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: String.Join(", ", ModelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage))),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }

                contentItem = newContentItem;
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
                {
                    return this.ChallengeOrForbid("Api");
                }

                contentItem.Merge(model, UpdateJsonMergeSettings);

                await _contentManager.UpdateAsync(contentItem);
                var result = await _contentManager.ValidateAsync(contentItem);

                if (!result.Succeeded)
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: string.Join(',', result.Errors),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
                // We check the model state after calling all handlers because they trigger WF content events so, even they are not
                // intended to add model errors (only drivers), a WF content task may be executed inline and add some model errors.
                else if (!ModelState.IsValid)
                {
                    return Problem(
                        title: S["One or more validation errors occurred."],
                        detail: String.Join(", ", ModelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage))),
                        statusCode: (int)HttpStatusCode.BadRequest);
                }
            }

            if (!draft)
            {
                await _contentManager.PublishAsync(contentItem);
            }
            else
            {
                await _contentManager.SaveDraftAsync(contentItem);
            }

            return Ok(contentItem);
        }
    }
}
