using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Contents.Endpoints.Api;

internal static partial class ContentManagementApiEndpoints
{
    private static void AddVersionEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/{contentItemId}/versions", ListVersionsAsync)
            .WithName("ApiListContentItemVersions").WithTags("Content Versions")
            .WithSummary("Lists readable versions of a content item, newest first.")
            .WithDescription("Includes published, draft, and archived versions. Paging and totals include only versions the caller can read.")
            .WithCliCommand(Cli(["content", "versions"], "list", arguments: [new("contentItemId", 0)],
                tableColumns: [new("items[].ContentItemVersionId", "Version ID"), new("items[].Latest", "Latest"), new("items[].Published", "Published"), new("items[].ModifiedUtc", "Modified"), new("items[].Author", "Author")]))
            .Produces<ContentItemsResponse>().ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
        group.MapGet("/versions/{contentItemVersionId}", GetVersionAsync)
            .WithName("ApiGetContentItemVersion").WithTags("Content Versions")
            .WithSummary("Gets the exact content item version by its version ID.")
            .WithDescription("Reads a stored version without creating a draft or changing its publication state.")
            .WithCliCommand(Cli(["content", "versions"], "show", arguments: [new("contentItemVersionId", 0)]))
            .Produces<ContentItem>().ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
        group.MapGet("/versions/{contentItemVersionId}/render", RenderVersionAsync)
            .WithName("ApiRenderContentItemVersion").WithTags("Content Versions")
            .WithSummary("Renders the selected content item version using current templates.")
            .WithDescription("Renders the exact stored content with the requested display type and the tenant's current theme and templates.")
            .WithCliCommand(Cli(["content", "versions"], "render", arguments: [new("contentItemVersionId", 0)]))
            .Produces<ContentItemRenderResponse>().ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
        group.MapPost("/versions/{contentItemVersionId}/restore", RestoreVersionAsync)
            .WithName("ApiRestoreContentItemVersion").WithTags("Content Versions")
            .WithSummary("Restores a version into a new unpublished draft.")
            .WithDescription("Creates a new version ID on every successful request. Existing drafts require replaceDraft=true. The source and published version remain unchanged.")
            .WithCliCommand(Cli(["content", "versions"], "restore", arguments: [new("contentItemVersionId", 0)]))
            .Produces<ContentItem>(201).ProducesValidationProblem().ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(409);
        group.MapDelete("/versions/{contentItemVersionId}", DeleteVersionAsync)
            .WithName("ApiDeleteContentItemVersion").WithTags("Content Versions")
            .WithSummary("Permanently deletes an archived content item version.")
            .WithDescription("Rejects latest or published versions. Does not delete the logical item, related content, media, or audit events.")
            .WithCliCommand(Cli(["content", "versions"], "delete", arguments: [new("contentItemVersionId", 0)], requiresConfirmation: true))
            .Produces<ContentItem>().Produces(204).ProducesProblem(401).ProducesProblem(403).ProducesProblem(409);
    }

    private static async Task<IResult> ListVersionsAsync(string contentItemId, IContentManager manager, ISession session,
        ContentApiService service, HttpContext httpContext, int skip = 0, int take = 20)
    {
        if (!await HasAccessAsync(httpContext))
        {
            return httpContext.ApiForbidProblem();
        }
        var current = await GetCurrentVersionAsync(contentItemId, manager, session);
        if (current is null)
        {
            return httpContext.ApiNotFoundProblem();
        }
        var auth = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        if (!await auth.AuthorizeAsync(httpContext.User, CommonPermissions.ListContent, current))
        {
            return httpContext.ApiForbidProblem();
        }
        var response = new ContentItemsResponse { Skip = Math.Max(0, skip), Take = Math.Clamp(take, 1, 100) };
        for (var offset = 0; ; offset += 100)
        {
            var batch = (await session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == contentItemId)
                .OrderByDescending(x => x.DocumentId).Skip(offset).Take(100).ListAsync()).ToArray();
            foreach (var version in batch)
            {
                if (!await CanReadVersionAsync(httpContext, version, current))
                {
                    continue;
                }
                if (response.TotalCount >= response.Skip && response.Items.Count < response.Take)
                {
                    response.Items.Add(version);
                }
                response.TotalCount++;
            }
            if (batch.Length < 100)
            {
                break;
            }
        }
        return Results.Json(response, service.SerializerOptions);
    }

    private static async Task<IResult> GetVersionAsync(string contentItemVersionId, IContentManager manager, ISession session, ContentApiService service, HttpContext httpContext)
    {
        var (version, _, error) = await AuthorizeVersionAsync(contentItemVersionId, manager, session, httpContext);
        return error ?? Results.Json(version, service.SerializerOptions);
    }

    private static async Task<IResult> DeleteVersionAsync(string contentItemVersionId, IContentManager manager, ISession session, ContentApiService service, HttpContext httpContext)
    {
        var (version, _, error) = await AuthorizeVersionAsync(contentItemVersionId, manager, session, httpContext, CommonPermissions.DeleteContent);
        if (error is not null)
        {
            return error is IStatusCodeHttpResult { StatusCode: 404 } ? TypedResults.NoContent() : error;
        }
        if (version.Latest || version.Published)
        {
            return TypedResults.Problem(statusCode: 409, detail: "Only archived versions can be permanently deleted. This version is latest or published; use the content item lifecycle commands first.");
        }
        // Match archived-version pruning. Removing the logical content item here
        // would invoke lifecycle handlers for its active versions and related data.
        session.Delete(version);
        return Results.Json(version, service.SerializerOptions);
    }

    private static async Task<IResult> RestoreVersionAsync(string contentItemVersionId, IContentManager manager, ISession session,
        ContentApiService service, HttpContext httpContext, bool replaceDraft = false)
    {
        var (version, current, error) = await AuthorizeVersionAsync(contentItemVersionId, manager, session, httpContext, CommonPermissions.PublishContent);
        if (error is not null)
        {
            return error;
        }
        var auth = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        if (!await auth.AuthorizeAsync(httpContext.User, CommonPermissions.EditContent, current)
            || !await auth.AuthorizeAsync(httpContext.User, CommonPermissions.EditContent, version))
        {
            return httpContext.ApiForbidProblem();
        }
        if (current.Latest && !current.Published && !replaceDraft)
        {
            return TypedResults.Problem(statusCode: 409, detail: "A current draft already exists. Inspect it first, then use replaceDraft=true to replace it with a restored draft.");
        }
        var errors = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        await service.ValidatePayloadAsync(null, version, version.ContentType, errors);
        if (!errors.IsValid)
        {
            return ValidationProblem(errors);
        }

        // RestoreAsync changes IDs and flags: never pass the tracked historical
        // document. Preserve current ownership instead of restoring an old owner.
        var restored = version.Clone();
        restored.Owner = current.Owner;
        var validation = await manager.RestoreAsync(restored);
        if (!validation.Succeeded)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { [string.Empty] = validation.Errors.Select(x => x.ErrorMessage).ToArray() });
        }
        return Results.Json(restored, service.SerializerOptions, statusCode: StatusCodes.Status201Created);
    }

    private static async Task<IResult> RenderVersionAsync(string contentItemVersionId, IContentManager manager, ISession session,
        HttpContext httpContext, string displayType = "Detail")
    {
        var (version, _, error) = await AuthorizeVersionAsync(contentItemVersionId, manager, session, httpContext);
        if (error is not null)
        {
            return error;
        }
        var services = httpContext.RequestServices;
        var shape = await services.GetRequiredService<IContentItemDisplayManager>().BuildDisplayAsync(version,
            services.GetRequiredService<IUpdateModelAccessor>().ModelUpdater, displayType ?? "Detail");
        var html = await services.GetRequiredService<IDisplayHelper>().ShapeExecuteAsync(shape);
        using var writer = new StringWriter();
        html.WriteTo(writer, HtmlEncoder.Default);
        return TypedResults.Ok(new ContentItemRenderResponse
        {
            ContentItemId = version.ContentItemId, ContentItemVersionId = version.ContentItemVersionId,
            DisplayType = displayType ?? "Detail", Html = writer.ToString(),
        });
    }

    private static async Task<(ContentItem Version, ContentItem Current, IResult Error)> AuthorizeVersionAsync(string versionId,
        IContentManager manager, ISession session, HttpContext context, Permission mutationPermission = null)
    {
        if (!await HasAccessAsync(context))
        {
            return (null, null, context.ApiForbidProblem());
        }
        var version = await manager.GetVersionAsync(versionId);
        if (version is null)
        {
            return (null, null, context.ApiNotFoundProblem());
        }
        var current = await GetCurrentVersionAsync(version.ContentItemId, manager, session) ?? version;
        var auth = context.RequestServices.GetRequiredService<IAuthorizationService>();
        if (!await CanReadVersionAsync(context, version, current)
            || mutationPermission is not null && (!await auth.AuthorizeAsync(context.User, mutationPermission, version)
                || !await auth.AuthorizeAsync(context.User, mutationPermission, current)))
        {
            return (null, null, context.ApiForbidProblem());
        }
        return (version, current, null);
    }

    private static async Task<bool> CanReadVersionAsync(HttpContext context, ContentItem version, ContentItem current)
    {
        var auth = context.RequestServices.GetRequiredService<IAuthorizationService>();
        var permission = version.Published ? CommonPermissions.ViewContent : CommonPermissions.PreviewContent;
        return await auth.AuthorizeAsync(context.User, permission, version)
            && await auth.AuthorizeAsync(context.User, permission, current);
    }

    private static async Task<ContentItem> GetCurrentVersionAsync(string itemId, IContentManager manager, ISession session)
        => await manager.GetAsync(itemId, VersionOptions.Latest)
            ?? await session.Query<ContentItem, ContentItemIndex>(x => x.ContentItemId == itemId)
                .OrderByDescending(x => x.DocumentId).FirstOrDefaultAsync();
}
