using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Templates.Deployment;
using OrchardCore.Templates.Services;

namespace OrchardCore.Templates.Endpoints.Management;

internal static class TemplateExportEndpoints
{
    internal static void AddTemplateExportEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup(TemplateManagementEndpointConventions.TagName, DeploymentPermissions.Export, Permissions.ManageTemplates, OrchardCore.Liquid.Permissions.ManageLiquidTemplates)
            .MapGet("api/templates/{name}:export", ExportAsync)
            .WithName("ApiExportTemplate")
            .WithSummary("Exports a custom template as a recipe.")
            .WithDescription("Returns a recipe with one step, built by the deployment source of the all-items step.")
            .WithCliCommand(new CliOperationMetadata(["templates"], "export")
            {
                Capability = TemplateManagementEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Produces<JsonObject>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAsync(string name, HttpContext context, [FromServices] TemplatesManager templates, [FromServices] IDeploymentManager deployment)
    {
        var document = await templates.GetTemplatesDocumentAsync();

        return TemplateManagementEndpoints.TryGetDefinition(document, name, out var definition)
            ? TypedResults.Ok(await deployment.ExportRecipeAsync(new AllTemplatesDeploymentStep { Names = [definition.Name] }, "Template-" + definition.Name, context.User))
            : context.ApiNotFoundProblem();
    }
}
