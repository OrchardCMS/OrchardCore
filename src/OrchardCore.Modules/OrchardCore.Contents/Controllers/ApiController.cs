using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Contents.Controllers
{
    [Route("api/content")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken, AllowAnonymous]
    public class ApiController : Controller
    {
        private static readonly JsonMergeSettings _updateJsonMergeSettings = new() { MergeArrayHandling = MergeArrayHandling.Replace };

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAuthorizationService _authorizationService;
        protected readonly IStringLocalizer S;

        public ApiController(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IAuthorizationService authorizationService,
            IStringLocalizer<ApiController> stringLocalizer)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _authorizationService = authorizationService;
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
                return NoContent();
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
                if (String.IsNullOrEmpty(model?.ContentType) || _contentDefinitionManager.GetTypeDefinition(model.ContentType) == null)
                {
                    return BadRequest();
                }

                contentItem = await _contentManager.NewAsync(model.ContentType);
                contentItem.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
                {
                    return this.ChallengeOrForbid("Api");
                }

                contentItem.Merge(model);

                var result = await _contentManager.UpdateValidateAndCreateAsync(contentItem, VersionOptions.Draft);

                if (!result.Succeeded)
                {
                    // Add the validation results to the ModelState to present the errors as part of the response.
                    AddValidationErrorsToModelState(result);
                }

                // We check the model state after calling all handlers because they trigger WF content events so, even they are not
                // intended to add model errors (only drivers), a WF content task may be executed inline and add some model errors.
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(new ValidationProblemDetails(ModelState)
                    {
                        Title = S["One or more validation errors occurred."],
                        Detail = String.Join(", ", ModelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage))),
                        Status = (int)HttpStatusCode.BadRequest,
                    });
                }
            }
            else
            {
                if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
                {
                    return this.ChallengeOrForbid("Api");
                }

                contentItem.Merge(model, _updateJsonMergeSettings);

                await _contentManager.UpdateAsync(contentItem);
                var result = await _contentManager.ValidateAsync(contentItem);

                if (!result.Succeeded)
                {
                    // Add the validation results to the ModelState to present the errors as part of the response.
                    AddValidationErrorsToModelState(result);
                }

                // We check the model state after calling all handlers because they trigger WF content events so, even they are not
                // intended to add model errors (only drivers), a WF content task may be executed inline and add some model errors.
                if (!ModelState.IsValid)
                {
                    return ValidationProblem(new ValidationProblemDetails(ModelState)
                    {
                        Title = S["One or more validation errors occurred."],
                        Detail = String.Join(", ", ModelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage))),
                        Status = (int)HttpStatusCode.BadRequest,
                    });
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

        private void AddValidationErrorsToModelState(ContentValidateResult result)
        {
            foreach (var error in result.Errors)
            {
                if (error.MemberNames != null && error.MemberNames.Any())
                {
                    foreach (var memberName in error.MemberNames)
                    {
                        ModelState.AddModelError(memberName, error.ErrorMessage);
                    }
                }
                else
                {
                    ModelState.AddModelError(String.Empty, error.ErrorMessage);
                }
            }
        }
    }
}
