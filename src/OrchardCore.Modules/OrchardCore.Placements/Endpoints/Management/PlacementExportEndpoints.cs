using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.Placements.Deployment;
using OrchardCore.Placements.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Placements.Endpoints.Management;

internal static class PlacementExportEndpoints
{
    internal static void AddPlacementExportEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup("Placements", DeploymentPermissions.Export, Permissions.ManagePlacements)
            .MapGet("api/placements/by-shape:export", ExportAsync)
            .WithName("ApiExportPlacement")
            .WithSummary("Exports the stored rules of a shape type as a recipe.")
            .WithDescription("Returns a recipe with one step, built by the deployment source of the all-items step.")
            .WithCliCommand(new CliOperationMetadata(["placements"], "export")
            {
                Capability = PlacementManagementEndpoints.CapabilityName,
                Arguments = { new CliArgumentMetadata("shapeType", 0) },
            })
            .Produces<JsonObject>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAsync([FromQuery] string shapeType, HttpContext context, [FromServices] PlacementsManager placements, [FromServices] IDeploymentManager deployment)
    {
        var definition = await PlacementManagementEndpoints.FindAsync(placements, shapeType);

        return definition is null
            ? context.ApiNotFoundProblem()
            : TypedResults.Ok(await deployment.ExportRecipeAsync(new PlacementsDeploymentStep { Names = [definition.ShapeType] }, "Placement-" + definition.ShapeType, context.User));
    }
}
