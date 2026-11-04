using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.RemoteManagement;

namespace OrchardCore.ContentLocalization.Endpoints;

internal static class ContentLocalizationEndpoints
{
    internal const string CapabilityName = "content-localizations";

    public static void AddContentLocalizationEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/content/{contentItemId}/localizations", ListAsync), "ApiListContentLocalizations", "Lists authorized localized variants of a content item.", "list")
            .Produces<ContentLocalizationResponse[]>();
        Configure(routes.MapPost("api/content/{contentItemId}/localizations", LocalizeAsync), "ApiLocalizeContent", "Creates a localized draft or reuses an existing editable variant without publishing it.", "create", input: true)
            .Accepts<LocalizeContentRequest>("application/json").Produces<LocalizeContentResponse>().ProducesProblem(409);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string summary, string verb, bool input = false)
    {
        var metadata = new CliOperationMetadata(["content", "localizations"], verb)
        {
            Capability = CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
        };
        metadata.Arguments.Add(new CliArgumentMetadata("contentItemId", 0));
        return builder.WithName(operationId).WithTags("Content Localizations").WithSummary(summary).WithCliCommand(metadata)
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IContentManager content, [FromServices] IContentLocalizationManager localizations,
        string contentItemId, string version = "latest")
    {
        if (!await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement))
        {
            return context.ApiForbidProblem();
        }
        if (version is not ("latest" or "published"))
        {
            return TypedResults.Problem("Version must be latest or published.", statusCode: 400);
        }
        var options = version == "published" ? VersionOptions.Published : VersionOptions.Latest;
        var source = await content.GetAsync(contentItemId, options);
        if (source is null)
        {
            return context.ApiNotFoundProblem();
        }
        if (!await CanReadAsync(context, authorization, source))
        {
            return context.ApiForbidProblem();
        }
        if (!source.TryGet<LocalizationPart>(out var part))
        {
            return context.ApiNotFoundProblem();
        }
        var variants = string.IsNullOrEmpty(part.LocalizationSet) ? [source] : await localizations.GetItemsForSetAsync(part.LocalizationSet);
        var visible = new List<ContentLocalizationResponse>();
        foreach (var id in variants.Select(item => item.ContentItemId).Distinct(StringComparer.Ordinal))
        {
            var item = await content.GetAsync(id, options);
            // A newer draft can have changed set membership; never expose it through the old set.
            if (item is not null && item.TryGet<LocalizationPart>(out var candidate)
                && candidate.LocalizationSet == part.LocalizationSet && await CanReadAsync(context, authorization, item))
            {
                visible.Add(ToResponse(item));
            }
        }
        return TypedResults.Ok(visible.OrderBy(item => item.Culture, StringComparer.OrdinalIgnoreCase)
            .ThenBy(item => item.ContentItemId, StringComparer.Ordinal).ToArray());
    }

    internal static async Task<IResult> LocalizeAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IContentLocalizationService localizations, string contentItemId, [FromBody] LocalizeContentRequest request)
    {
        if (!await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement))
        {
            return context.ApiForbidProblem();
        }
        if (request?.Culture is null)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["culture"] = ["A configured culture name is required."] });
        }
        var result = await localizations.LocalizeAsync(context.User, contentItemId, request.Culture);
        return result.Status switch
        {
            ContentLocalizationStatus.NotFound => context.ApiNotFoundProblem(),
            ContentLocalizationStatus.Forbidden => context.ApiForbidProblem(),
            ContentLocalizationStatus.Conflict => TypedResults.Problem("The existing variant has a latest draft with different localization membership. Resolve that draft before localizing this culture.", statusCode: 409),
            ContentLocalizationStatus.Invalid => TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["culture"] = ["Select a configured culture."] }),
            _ => TypedResults.Ok(new LocalizeContentResponse { Item = ToResponse(result.ContentItem), Created = result.Created }),
        };
    }

    private static Task<bool> CanReadAsync(HttpContext context, IAuthorizationService authorization, ContentItem item) =>
        authorization.AuthorizeAsync(context.User, item.Published ? CommonPermissions.ViewContent : CommonPermissions.PreviewContent, item);

    private static ContentLocalizationResponse ToResponse(ContentItem item)
    {
        var part = item.Get<LocalizationPart>(nameof(LocalizationPart));
        return new()
        {
            ContentItemId = item.ContentItemId, ContentItemVersionId = item.ContentItemVersionId,
            ContentType = item.ContentType, DisplayText = item.DisplayText, LocalizationSet = part.LocalizationSet,
            Culture = part.Culture, Published = item.Published, Latest = item.Latest,
        };
    }
}
