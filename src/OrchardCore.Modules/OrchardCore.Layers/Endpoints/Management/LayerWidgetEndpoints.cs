using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Layers.Endpoints.Management;

internal static class LayerWidgetEndpoints
{
    internal const string CapabilityName = "layer-widgets";

    public static void AddLayerWidgetEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/layer-widgets", ListAsync), "ApiListLayerWidgets", "Lists authorized widget placements.", "list")
            .Produces<LayerWidgetListResponse>();
        Configure(routes.MapGet("api/layer-widget-zones", ZonesAsync), "ApiListLayerWidgetZones", "Lists configured tenant zones for widgets.", "zones")
            .Produces<string[]>();
        Configure(routes.MapGet("api/layer-widgets/{contentItemId}", GetAsync), "ApiGetLayerWidget", "Shows a widget placement.", "show", argument: true)
            .Produces<LayerWidgetResponse>().ProducesProblem(404);
        Configure(routes.MapPut("api/layer-widgets/{contentItemId}", UpdateAsync), "ApiUpdateLayerWidget", "Attaches or replaces widget placement without publishing draft content.", "update", argument: true, input: true)
            .Accepts<LayerWidgetPlacement>("application/json").Produces<LayerWidgetPlacement>().ProducesProblem(404);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string summary,
        string verb, bool argument = false, bool input = false)
    {
        var metadata = new CliOperationMetadata(["layers", "widgets"], verb)
        {
            Capability = CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
        };
        if (argument)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("contentItemId", 0));
        }
        return builder.WithName(operationId).WithTags("Layer Widgets").WithSummary(summary)
            .WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(Permissions.ManageLayers)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ZonesAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerWidgetService widgets) => !await authorization.AuthorizeAsync(context.User, Permissions.ManageLayers)
            ? context.ApiForbidProblem() : TypedResults.Ok(await widgets.GetZonesAsync());

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerService layers, [AsParameters] LayerWidgetListRequest request)
    {
        if (!await authorization.AuthorizeAsync(context.User, Permissions.ManageLayers))
        {
            return context.ApiForbidProblem();
        }
        var version = request.Version ?? "latest";
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (version is not ("latest" or "published") || skip < 0 || take < 1 || take > 200)
        {
            return TypedResults.Problem("Version must be latest or published, skip nonnegative and take between 1 and 200.", statusCode: 400);
        }
        var metadata = version == "published"
            ? await layers.GetLayerWidgetsMetadataAsync(item => item.Published)
            : await layers.GetLayerWidgetsMetadataAsync(item => item.Latest);
        var visible = new List<LayerWidgetResponse>();
        foreach (var placement in metadata)
        {
            if ((request.Layer is null || string.Equals(request.Layer, placement.Layer, StringComparison.OrdinalIgnoreCase))
                && (request.Zone is null || request.Zone == placement.Zone)
                && await CanViewAsync(context, authorization, placement.ContentItem))
            {
                visible.Add(ToResponse(placement));
            }
        }
        return TypedResults.Ok(new LayerWidgetListResponse
        {
            Skip = skip, Take = take, TotalCount = visible.Count,
            Items = visible.OrderBy(item => item.Placement.Zone, StringComparer.Ordinal)
                .ThenBy(item => item.Placement.Position).ThenBy(item => item.ContentItemId, StringComparer.Ordinal)
                .Skip(skip).Take(take).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IContentManager manager, string contentItemId, string version = "latest")
    {
        if (!await authorization.AuthorizeAsync(context.User, Permissions.ManageLayers))
        {
            return context.ApiForbidProblem();
        }
        if (version is not ("latest" or "published"))
        {
            return TypedResults.Problem("Version must be latest or published.", statusCode: 400);
        }
        var item = await manager.GetAsync(contentItemId, version == "published" ? VersionOptions.Published : VersionOptions.Latest);
        if (item is null)
        {
            return context.ApiNotFoundProblem();
        }
        if (!await CanViewAsync(context, authorization, item))
        {
            return context.ApiForbidProblem();
        }
        return item.TryGet<LayerMetadata>(out var placement) ? TypedResults.Ok(ToResponse(placement)) : context.ApiNotFoundProblem();
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] ILayerWidgetService widgets, string contentItemId, [FromBody] LayerWidgetPlacement placement)
    {
        if (!await authorization.AuthorizeAsync(context.User, Permissions.ManageLayers))
        {
            return context.ApiForbidProblem();
        }
        var result = await widgets.UpdateAsync(context.User, contentItemId, placement?.ToMetadata());
        return result.Status switch
        {
            LayerWidgetMutationStatus.Forbidden => context.ApiForbidProblem(),
            LayerWidgetMutationStatus.NotFound => context.ApiNotFoundProblem(),
            LayerWidgetMutationStatus.Invalid => TypedResults.ValidationProblem(result.Errors),
            _ => TypedResults.Ok(ToPlacement(result.Placement)),
        };
    }

    private static Task<bool> CanViewAsync(HttpContext context, IAuthorizationService authorization, ContentItem item) =>
        authorization.AuthorizeAsync(context.User, item.Published ? CommonPermissions.ViewContent : CommonPermissions.PreviewContent, item);

    private static LayerWidgetPlacement ToPlacement(LayerMetadata placement) => new()
    {
        Layer = placement.Layer, Zone = placement.Zone, Position = placement.Position, RenderTitle = placement.RenderTitle,
    };

    private static LayerWidgetResponse ToResponse(LayerMetadata placement) => new()
    {
        ContentItemId = placement.ContentItem.ContentItemId,
        ContentItemVersionId = placement.ContentItem.ContentItemVersionId,
        ContentType = placement.ContentItem.ContentType,
        DisplayText = placement.ContentItem.DisplayText,
        Published = placement.ContentItem.Published,
        Placement = ToPlacement(placement),
    };
}
