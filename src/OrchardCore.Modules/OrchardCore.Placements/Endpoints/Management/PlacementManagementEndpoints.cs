using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Placements.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Placements.Endpoints.Management;

internal static class PlacementManagementEndpoints
{
    private const string RoutePrefix = "api/placements";
    internal const string CapabilityName = "placements";

    public static void AddPlacementManagementEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet(RoutePrefix, ListAsync), "ApiListPlacements", "Lists stored shape placement rules.", "list")
            .Produces<PlacementListResponse>();
        Configure(routes.MapGet(RoutePrefix + "/by-shape", GetAsync), "ApiGetPlacement", "Shows stored rules for a shape type.", "show", argument: true)
            .Produces<PlacementDefinition>().ProducesProblem(404);
        Configure(routes.MapGet("api/placement-filters", FiltersAsync), "ApiListPlacementFilters", "Lists placement filter keys registered by enabled features.", "filters")
            .Produces<IReadOnlyList<string>>();
        Configure(routes.MapPost(RoutePrefix + "/validate", ValidateAsync), "ApiValidatePlacement", "Validates placement rules without rendering or saving.", "validate", input: true)
            .Accepts<PlacementDefinition>("application/json").Produces<PlacementValidationResponse>();
        Configure(routes.MapPost(RoutePrefix, CreateAsync), "ApiCreatePlacement", "Creates placement rules; equivalent stable-shape retries return the stored rules.", "create", input: true)
            .Accepts<PlacementDefinition>("application/json").Produces<PlacementDefinition>(201).ProducesProblem(409);
        Configure(routes.MapPut(RoutePrefix + "/by-shape", UpdateAsync), "ApiUpdatePlacement", "Replaces complete placement rules; an empty array removes the override.", "update", argument: true, input: true)
            .Accepts<PlacementDefinition>("application/json").Produces<PlacementDefinition>().Produces(204).ProducesProblem(404);
        Configure(routes.MapDelete(RoutePrefix + "/by-shape", DeleteAsync), "ApiDeletePlacement", "Removes stored placement rules; absent rules are a successful no-op.", "delete", argument: true, confirmation: true)
            .Produces(204);
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operationId, string summary,
        string verb, bool argument = false, bool input = false, bool confirmation = false)
    {
        var metadata = new CliOperationMetadata(["placements"], verb)
        {
            Capability = CapabilityName,
            InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = confirmation,
        };
        if (argument)
        {
            metadata.Arguments.Add(new CliArgumentMetadata("shapeType", 0));
        }
        return builder.WithName(operationId).WithTags("Placements").WithSummary(summary)
            .WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser()
                .AddRequirements(new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(Permissions.ManagePlacements)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager, [AsParameters] PlacementListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take < 1 || take > 200)
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var placements = await manager.ListShapePlacementsAsync();
        var matches = placements.Where(entry => string.IsNullOrWhiteSpace(request.Search)
            || entry.Key.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase).ToArray();
        return TypedResults.Ok(new PlacementListResponse
        {
            Skip = skip, Take = take, TotalCount = matches.Length,
            Items = matches.Skip(skip).Take(take).Select(entry => new PlacementDefinition { ShapeType = entry.Key, Nodes = entry.Value }).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager, [FromQuery] string shapeType)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var placements = await manager.ListShapePlacementsAsync();
        var entry = placements.FirstOrDefault(entry => string.Equals(entry.Key, shapeType, StringComparison.OrdinalIgnoreCase));
        return entry.Key is null ? context.ApiNotFoundProblem() : TypedResults.Ok(new PlacementDefinition { ShapeType = entry.Key, Nodes = entry.Value });
    }

    internal static async Task<IResult> FiltersAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager) => !await AuthorizedAsync(context, authorization)
            ? context.ApiForbidProblem() : TypedResults.Ok(manager.GetFilterKeys());

    internal static async Task<IResult> ValidateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager, [FromBody] PlacementDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        var errors = manager.Validate(definition?.ShapeType, definition?.Nodes);
        return TypedResults.Ok(new PlacementValidationResponse { IsValid = errors.Count == 0, Errors = errors });
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager, [FromBody] PlacementDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        if (definition?.Nodes?.Length == 0)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]> { ["nodes"] = ["Provide at least one rule when creating a placement override."] });
        }
        var result = await manager.SaveAsync(definition?.ShapeType, definition?.Nodes, creating: true);
        if (result.Status == PlacementMutationStatus.Invalid)
        {
            return TypedResults.ValidationProblem(result.Errors);
        }
        if (result.Status == PlacementMutationStatus.Conflict)
        {
            return TypedResults.Problem("This shape type already has a different placement definition.", statusCode: 409);
        }
        return TypedResults.Created($"{context.Request.PathBase}/{RoutePrefix}/by-shape?shapeType={Uri.EscapeDataString(result.ShapeType)}",
            new PlacementDefinition { ShapeType = result.ShapeType, Nodes = result.Nodes });
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager, [FromQuery] string shapeType, [FromBody] PlacementDefinition definition)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        if (definition is null || !string.Equals(shapeType, definition.ShapeType, StringComparison.Ordinal))
        {
            return TypedResults.Problem("The body shapeType must match the shapeType query parameter. Renames are not supported.", statusCode: 400);
        }
        var result = await manager.SaveAsync(shapeType, definition.Nodes, requireExisting: true);
        if (result.Status == PlacementMutationStatus.Invalid)
        {
            return TypedResults.ValidationProblem(result.Errors);
        }
        if (result.Status == PlacementMutationStatus.Deleted)
        {
            return TypedResults.NoContent();
        }
        return result.Status == PlacementMutationStatus.NotFound ? context.ApiNotFoundProblem()
            : TypedResults.Ok(new PlacementDefinition { ShapeType = result.ShapeType, Nodes = result.Nodes });
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] PlacementsManager manager, [FromQuery] string shapeType)
    {
        if (!await AuthorizedAsync(context, authorization))
        {
            return context.ApiForbidProblem();
        }
        await manager.RemoveShapePlacementsAsync(shapeType);
        return TypedResults.NoContent();
    }

    private static Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        authorization.AuthorizeAsync(context.User, Permissions.ManagePlacements);
}
