using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.Contents.Endpoints.Api;

internal static partial class ContentManagementApiEndpoints
{
    internal const string Capability = "content-items";

    public static IEndpointRouteBuilder AddContentManagementApiEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/content")
            .RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = OrchardCoreConstants.AuthenticationSchemes.Api })
            .RequireAuthorization(policy => policy.AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .DisableAntiforgery();

        group.MapGet(string.Empty, ListAsync)
            .WithName("ApiListContentItems")
            .WithTags("Content Items")
            .WithSummary("Lists content items.")
            .WithDescription("Lists published, latest, or draft content items that the current user can access.")
            .WithCliCommand(Cli(["content", "items"], "list"))
            .Produces<ContentItemsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/{contentItemId}", GetAsync)
            .WithName("ApiGetContentItem")
            .WithTags("Content Items")
            .WithSummary("Gets a content item.")
            .WithDescription("Gets a published, latest, or draft content item version.")
            .WithCliCommand(Cli(["content", "items"], "show", arguments: [new CliArgumentMetadata("contentItemId", 0)]))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost(string.Empty, SaveAsync)
            .WithName("ApiSaveContentItem")
            .WithTags("Content Items")
            .WithSummary("Creates or updates a content item.")
            .WithDescription("Creates a content item when the request has no contentItemId, or updates the current draft for backward compatibility when one is supplied.")
            .WithCliCommand(Cli(["content", "items"], "save", inputMode: CliInputMode.Json))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/draft", CreateDraftAsync)
            .WithName("ApiCreateDraftContentItem")
            .WithTags("Content Items")
            .WithSummary("Creates a draft content item.")
            .WithDescription("Creates a new draft content item.")
            .WithCliCommand(Cli(["content", "items"], "create-draft", inputMode: CliInputMode.Json))
            .Produces<ContentItem>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{contentItemId}", UpdateAsync)
            .WithName("ApiUpdateContentItem")
            .WithTags("Content Items")
            .WithSummary("Updates and publishes a content item.")
            .WithDescription("Updates an existing draft version and publishes it.")
            .WithCliCommand(Cli(["content", "items"], "update", arguments: [new CliArgumentMetadata("contentItemId", 0)], inputMode: CliInputMode.Json))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{contentItemId}/draft", UpdateDraftAsync)
            .WithName("ApiUpdateDraftContentItem")
            .WithTags("Content Items")
            .WithSummary("Updates a draft content item.")
            .WithDescription("Updates an existing draft version and keeps it in draft state.")
            .WithCliCommand(Cli(["content", "items"], "update-draft", arguments: [new CliArgumentMetadata("contentItemId", 0)], inputMode: CliInputMode.Json))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{contentItemId}/draft", CreateDraftVersionAsync)
            .WithName("ApiSaveDraftContentItem")
            .WithTags("Content Items")
            .WithSummary("Creates or refreshes a draft version.")
            .WithDescription("Creates or refreshes the draft version for an existing content item.")
            .WithCliCommand(Cli(["content", "items"], "draft", arguments: [new CliArgumentMetadata("contentItemId", 0)]))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{contentItemId}/publish", PublishAsync)
            .WithName("ApiPublishContentItem")
            .WithTags("Content Items")
            .WithSummary("Publishes a content item.")
            .WithDescription("Publishes the current draft version of a content item.")
            .WithCliCommand(Cli(["content", "items"], "publish", arguments: [new CliArgumentMetadata("contentItemId", 0)]))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{contentItemId}/unpublish", UnpublishAsync)
            .WithName("ApiUnpublishContentItem")
            .WithTags("Content Items")
            .WithSummary("Unpublishes a content item.")
            .WithDescription("Unpublishes the published version of a content item.")
            .WithCliCommand(Cli(["content", "items"], "unpublish", arguments: [new CliArgumentMetadata("contentItemId", 0)]))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{contentItemId}", DeleteAsync)
            .WithName("ApiDeleteContentItem")
            .WithTags("Content Items")
            .WithSummary("Deletes a content item.")
            .WithDescription("Deletes the latest version of a content item.")
            .WithCliCommand(Cli(["content", "items"], "delete", arguments: [new CliArgumentMetadata("contentItemId", 0)], requiresConfirmation: true))
            .Produces<ContentItem>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/validate", ValidateCreateAsync)
            .WithName("ApiValidateContentItem")
            .WithTags("Content Items")
            .WithSummary("Validates a content item payload.")
            .WithDescription("Validates a new content item or an existing draft payload without saving it.")
            .WithCliCommand(Cli(["content", "items"], "validate", inputMode: CliInputMode.Json))
            .Produces<ContentItemValidationResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{contentItemId}/validate", ValidateUpdateAsync)
            .WithName("ApiValidateContentItemUpdate")
            .WithTags("Content Items")
            .WithSummary("Validates an updated content item payload.")
            .WithDescription("Validates an update for an existing content item without saving it.")
            .WithCliCommand(Cli(["content", "items"], "validate-update", arguments: [new CliArgumentMetadata("contentItemId", 0)], inputMode: CliInputMode.Json))
            .Produces<ContentItemValidationResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{contentItemId}/render", RenderAsync)
            .WithName("ApiRenderContentItem")
            .WithTags("Content Items")
            .WithSummary("Renders a content item.")
            .WithDescription("Renders a content item with the requested display type when display services are available.")
            .WithCliCommand(Cli(["content", "items"], "render", arguments: [new CliArgumentMetadata("contentItemId", 0)]))
            .Produces<ContentItemRenderResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/schema/{contentType}", SchemaAsync)
            .WithName("ApiGetContentItemSchema")
            .WithTags("Content Items")
            .WithSummary("Gets a content item JSON schema.")
            .WithDescription("Returns a JSON schema derived from the current content definition and registered field and part types.")
            .WithCliCommand(Cli(["content", "items"], "schema", arguments: [new CliArgumentMetadata("contentType", 0)]))
            .Produces<ContentItemSchemaResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        AddVersionEndpoints(group);
        return builder;
    }

    private static async Task<IResult> ListAsync(ContentApiService service, HttpContext httpContext, string contentType = null, string status = "published", int skip = 0, int take = 20)
        => !await HasAccessAsync(httpContext)
            ? httpContext.ApiForbidProblem()
            : Results.Json(await service.ListAsync(httpContext.User, contentType, status, skip, take), service.SerializerOptions);

    private static async Task<IResult> GetAsync(string contentItemId, ContentApiService service, HttpContext httpContext, string version = "published")
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var contentItem = await service.GetAsync(contentItemId, version);
        if (contentItem is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        var permission = contentItem.Published ? CommonPermissions.ViewContent : CommonPermissions.PreviewContent;
        return !await httpContext.RequestServices.GetRequiredService<IAuthorizationService>().AuthorizeAsync(httpContext.User, permission, contentItem)
            ? httpContext.ApiForbidProblem()
            : Results.Json(contentItem, service.SerializerOptions);
    }

    private static Task<IResult> SaveAsync(JsonObject model, ContentApiService service, HttpContext httpContext, bool draft = false)
        => SaveCoreAsync(model, service, httpContext, publish: !draft, allowCreate: true, allowUpdate: true);

    private static Task<IResult> CreateDraftAsync(JsonObject model, ContentApiService service, HttpContext httpContext)
        => SaveCoreAsync(model, service, httpContext, publish: false, allowCreate: true, allowUpdate: false, createdOnSuccess: true);

    private static Task<IResult> UpdateAsync(string contentItemId, JsonObject model, ContentApiService service, HttpContext httpContext)
        => SaveCoreAsync(model, service, httpContext, publish: true, allowCreate: false, allowUpdate: true, contentItemId: contentItemId);

    private static Task<IResult> UpdateDraftAsync(string contentItemId, JsonObject model, ContentApiService service, HttpContext httpContext)
        => SaveCoreAsync(model, service, httpContext, publish: false, allowCreate: false, allowUpdate: true, contentItemId: contentItemId);

    private static async Task<IResult> CreateDraftVersionAsync(string contentItemId, ContentApiService service, HttpContext httpContext)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var contentItem = await service.CreateDraftAsync(httpContext.User, contentItemId);
        if (ContentApiService.IsForbidden(contentItem))
        {
            return httpContext.ApiForbidProblem();
        }

        return contentItem is null ? httpContext.ApiNotFoundProblem() : Results.Json(contentItem, service.SerializerOptions);
    }

    private static async Task<IResult> PublishAsync(string contentItemId, ContentApiService service, HttpContext httpContext)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var contentItem = await service.PublishAsync(httpContext.User, contentItemId);
        if (ContentApiService.IsForbidden(contentItem))
        {
            return httpContext.ApiForbidProblem();
        }

        return contentItem is null ? httpContext.ApiNotFoundProblem() : Results.Json(contentItem, service.SerializerOptions);
    }

    private static async Task<IResult> UnpublishAsync(string contentItemId, ContentApiService service, HttpContext httpContext)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var contentItem = await service.UnpublishAsync(httpContext.User, contentItemId);
        if (ContentApiService.IsForbidden(contentItem))
        {
            return httpContext.ApiForbidProblem();
        }

        return contentItem is null ? httpContext.ApiNotFoundProblem() : Results.Json(contentItem, service.SerializerOptions);
    }

    private static async Task<IResult> DeleteAsync(string contentItemId, ContentApiService service, HttpContext httpContext)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var contentItem = await service.DeleteAsync(httpContext.User, contentItemId);
        if (ContentApiService.IsForbidden(contentItem))
        {
            return httpContext.ApiForbidProblem();
        }

        return contentItem is null ? TypedResults.NoContent() : Results.Json(contentItem, service.SerializerOptions);
    }

    private static Task<IResult> ValidateCreateAsync(JsonObject model, ContentApiService service, HttpContext httpContext)
        => ValidateCoreAsync(model, null, service, httpContext);

    private static Task<IResult> ValidateUpdateAsync(string contentItemId, JsonObject model, ContentApiService service, HttpContext httpContext)
        => ValidateCoreAsync(model, contentItemId, service, httpContext);

    private static async Task<IResult> RenderAsync(string contentItemId, ContentApiService service, HttpContext httpContext, string version = "published", string displayType = "Detail")
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var response = await service.RenderAsync(httpContext.User, contentItemId, version, displayType);
        if (ContentApiService.IsForbidden(response))
        {
            return httpContext.ApiForbidProblem();
        }

        return response is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(response);
    }

    private static async Task<IResult> SchemaAsync(string contentType, ContentApiService service, HttpContext httpContext)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var auth = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        var isAuthorized = await auth.AuthorizeContentTypeAsync(httpContext.User, CommonPermissions.ListContent, contentType)
            || await auth.AuthorizeContentTypeAsync(httpContext.User, CommonPermissions.EditContent, contentType)
            || await auth.AuthorizeContentTypeAsync(httpContext.User, CommonPermissions.ViewContent, contentType);

        if (!isAuthorized)
        {
            return httpContext.ApiForbidProblem();
        }

        var schema = await service.GetSchemaAsync(contentType);
        return schema is null ? httpContext.ApiNotFoundProblem() : TypedResults.Ok(schema);
    }

    private static async Task<IResult> SaveCoreAsync(JsonObject model, ContentApiService service, HttpContext httpContext, bool publish, bool allowCreate, bool allowUpdate, bool createdOnSuccess = false, string contentItemId = null)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        if (model is null)
        {
            return TypedResults.Problem(title: "Bad request", detail: "A content item payload is required.", statusCode: StatusCodes.Status400BadRequest);
        }

        // Minimal API handlers do not run MVC's ModelBinderAccessorFilter. Keep one
        // updater so service validation and the HTTP response observe the same errors.
        var accessor = httpContext.RequestServices.GetRequiredService<IUpdateModelAccessor>();
        var updater = accessor.ModelUpdater;
        accessor.ModelUpdater = updater;
        var payload = ContentPayloadValidator.ReadPayload(model, updater.ModelState);
        if (!updater.ModelState.IsValid)
        {
            return ValidationProblem(updater.ModelState);
        }

        var contentItem = await service.SaveAsync(httpContext.User, payload, publish, allowCreate, allowUpdate, contentItemId, model);
        if (ContentApiService.IsForbidden(contentItem))
        {
            return httpContext.ApiForbidProblem();
        }

        if (contentItem is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        var modelState = updater.ModelState;
        if (!modelState.IsValid)
        {
            return ValidationProblem(modelState);
        }

        if (createdOnSuccess)
        {
            return TypedResults.Created($"/api/content/{contentItem.ContentItemId}", contentItem);
        }

        return Results.Json(contentItem, service.SerializerOptions);
    }

    private static async Task<IResult> ValidateCoreAsync(JsonObject model, string contentItemId, ContentApiService service, HttpContext httpContext)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }

        var errors = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        var payload = ContentPayloadValidator.ReadPayload(model, errors);
        if (!errors.IsValid)
        {
            return ValidationProblem(errors);
        }

        var response = await service.ValidateAsync(httpContext.User, payload, contentItemId, model);
        if (ContentApiService.IsForbidden(response))
        {
            return httpContext.ApiForbidProblem();
        }

        if (response is null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        return response.IsValid ? TypedResults.Ok(response) : TypedResults.ValidationProblem(response.Errors);
    }

    private static async Task<bool> HasAccessAsync(HttpContext httpContext)
        => await httpContext.RequestServices.GetRequiredService<IAuthorizationService>().AuthorizeAsync(httpContext.User, CommonPermissions.AccessContentApi);

    private static CliOperationMetadata Cli(
        string[] commandGroup,
        string verb,
        CliArgumentMetadata[] arguments = null,
        string[] aliases = null,
        CliTableColumnMetadata[] tableColumns = null,
        CliInputMode inputMode = CliInputMode.Options,
        bool requiresConfirmation = false)
    {
        var metadata = new CliOperationMetadata(commandGroup, verb)
        {
            Capability = Capability,
            InputMode = inputMode,
            RequiresConfirmation = requiresConfirmation,
        };

        if (arguments is not null)
        {
            foreach (var argument in arguments)
            {
                metadata.Arguments.Add(argument);
            }
        }

        if (aliases is not null)
        {
            foreach (var alias in aliases)
            {
                metadata.Aliases.Add(alias);
            }
        }

        if (tableColumns is not null)
        {
            foreach (var column in tableColumns)
            {
                metadata.TableColumns.Add(column);
            }
        }

        return metadata;
    }

    private static ValidationProblem ValidationProblem(ModelStateDictionary modelState)
        => TypedResults.ValidationProblem(modelState.ToDictionary(
            entry => entry.Key,
            entry => entry.Value?.Errors.Select(error => error.ErrorMessage).ToArray() ?? []),
            detail: string.Join(", ", modelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage))));
}
