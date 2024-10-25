using System.Security.Claims;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Json;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Endpoints.Api;

public static class CreateEndpoint
{
    private static readonly JsonMergeSettings _updateJsonMergeSettings = new()
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
    };

    public static IEndpointRouteBuilder AddCreateContentEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/content", HandleAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        ContentItem model,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        IContentDefinitionManager contentDefinitionManager,
        IUpdateModelAccessor updateModelAccessor,
        HttpContext httpContext,
        IOptions<DocumentJsonSerializerOptions> options,
        bool draft = false)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.AccessContentApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (model is null)
        {
            return TypedResults.BadRequest();
        }

        var contentItem = await contentManager.GetAsync(model.ContentItemId, VersionOptions.DraftRequired);
        var modelState = updateModelAccessor.ModelUpdater.ModelState;

        if (contentItem == null)
        {
            if (string.IsNullOrEmpty(model?.ContentType) || await contentDefinitionManager.GetTypeDefinitionAsync(model.ContentType) == null)
            {
                return TypedResults.BadRequest();
            }

            contentItem = await contentManager.NewAsync(model.ContentType);
            contentItem.Owner = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.PublishContent, contentItem))
            {
                return httpContext.ChallengeOrForbid("Api");
            }

            contentItem.Merge(model);

            var result = await contentManager.UpdateValidateAndCreateAsync(contentItem, VersionOptions.Draft);

            if (!result.Succeeded)
            {
                // Add the validation results to the ModelState to present the errors as part of the response.
                AddValidationErrorsToModelState(result, modelState);
            }

            // We check the model state after calling all handlers because they trigger WF content events so, even they are not
            // intended to add model errors (only drivers), a WF content task may be executed inline and add some model errors.
            if (!modelState.IsValid)
            {
                var errors = modelState.ToDictionary(entry => entry.Key, entry => entry.Value.Errors.Select(x => x.ErrorMessage).ToArray());

                return TypedResults.ValidationProblem(errors, detail: string.Join(", ", modelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage))));
            }
        }
        else
        {
            if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.EditContent, contentItem))
            {
                return httpContext.ChallengeOrForbid("Api");
            }

            contentItem.Merge(model, _updateJsonMergeSettings);

            await contentManager.UpdateAsync(contentItem);
            var result = await contentManager.ValidateAsync(contentItem);

            if (!result.Succeeded)
            {
                // Add the validation results to the ModelState to present the errors as part of the response.
                AddValidationErrorsToModelState(result, modelState);
            }

            // We check the model state after calling all handlers because they trigger WF content events so, even they are not
            // intended to add model errors (only drivers), a WF content task may be executed inline and add some model errors.
            if (!modelState.IsValid)
            {
                var errors = modelState.ToDictionary(entry => entry.Key, entry => entry.Value.Errors.Select(x => x.ErrorMessage).ToArray());

                return TypedResults.ValidationProblem(errors, detail: string.Join(", ", modelState.Values.SelectMany(x => x.Errors.Select(x => x.ErrorMessage))));
            }
        }

        if (!draft)
        {
            await contentManager.PublishAsync(contentItem);
        }
        else
        {
            await contentManager.SaveDraftAsync(contentItem);
        }

        return Results.Json(contentItem, options.Value.SerializerOptions);
    }

    private static void AddValidationErrorsToModelState(ContentValidateResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            if (error.MemberNames != null && error.MemberNames.Any())
            {
                foreach (var memberName in error.MemberNames)
                {
                    modelState.AddModelError(memberName, error.ErrorMessage);
                }
            }
            else
            {
                modelState.AddModelError(string.Empty, error.ErrorMessage);
            }
        }
    }
}
