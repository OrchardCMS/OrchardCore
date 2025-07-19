using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Json;
using OrchardCore.Routing.Extensions;
using OrchardCore.Modules;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Handlers;
using System.Text.Json.Settings;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Contents.Endpoints;

public sealed class ContentEndpoints : IEndpoint
{
    
    private static readonly JsonMergeSettings _updateJsonMergeSettings = new()
    {
        MergeArrayHandling = MergeArrayHandling.Replace,
    };

    public void Map(IEndpointRouteBuilder builder) => builder.MapGroup(this)
        .MapGet(GetContentAsync, "{contentItemId}")
        .MapPost(CreateContentAsync)
        .MapDelete(DeleteContentAsync, "{contentItemId}");

    private static async Task<IResult> GetContentAsync(
        string contentItemId,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IOptions<DocumentJsonSerializerOptions> options)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.AccessContentApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var contentItem = await contentManager.GetAsync(contentItemId);
        if (contentItem == null)
        {
            return TypedResults.NotFound();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.ViewContent, contentItem))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        return Results.Json(contentItem, options.Value.SerializerOptions);
    }

    private static async Task<IResult> CreateContentAsync(
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

            var result = await contentManager.ValidateAsync(contentItem);

            if (result.Succeeded)
            {
                await contentManager.CreateAsync(contentItem, VersionOptions.Draft);
            }
            else
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

    private static async Task<IResult> DeleteContentAsync(
        string contentItemId,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IOptions<DocumentJsonSerializerOptions> options)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.AccessContentApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var contentItem = await contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return TypedResults.NotFound();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.DeleteContent, contentItem))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        await contentManager.RemoveAsync(contentItem);

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
