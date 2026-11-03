using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.Layers.Deployment;
using OrchardCore.Layers.Services;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Layers.Endpoints.Management;

internal static class LayerExportEndpoints
{
    internal static void AddLayerExportEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup("Layers", DeploymentPermissions.Export, Permissions.ManageLayers)
            .MapGet("api/layers/by-name:export", ExportAsync)
            .WithName("ApiExportLayer")
            .WithSummary("Exports a layer definition as a recipe.")
            .WithDescription("Returns a recipe with one step, built by the deployment source of the all-items step.")
            .WithCliCommand(new CliOperationMetadata(["layers"], "export")
            {
                Capability = LayerManagementEndpoints.CapabilityName,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Produces<JsonObject>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAsync([FromQuery] string name, HttpContext context, [FromServices] ILayerService layers, [FromServices] IDeploymentManager deployment)
    {
        var layer = await layers.GetLayerAsync(name);

        return layer is null
            ? context.ApiNotFoundProblem()
            : TypedResults.Ok(await deployment.ExportRecipeAsync(new AllLayersDeploymentStep { Names = [layer.Name] }, "Layer-" + layer.Name, context.User));
    }
}
