using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Shortcodes.Deployment;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Shortcodes.Endpoints.Management;

internal static class ShortcodeTemplateExportEndpoints
{
    internal static void AddShortcodeTemplateExportEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup("Shortcode Templates", DeploymentPermissions.Export, ShortcodesPermissions.ManageShortcodeTemplates)
            .MapGet("api/shortcode-templates/by-name:export", ExportAsync)
            .WithName("ApiExportShortcodeTemplate")
            .WithSummary("Exports a shortcode template as a recipe.")
            .WithDescription("Returns a recipe with one step, built by the deployment source of the all-items step.")
            .WithCliCommand(new CliOperationMetadata(["shortcodes", "templates"], "export")
            {
                Capability = ShortcodeTemplateManagementEndpoints.CapabilityName,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Produces<JsonObject>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAsync([FromQuery] string name, HttpContext context, [FromServices] ShortcodeTemplatesManager templates, [FromServices] IDeploymentManager deployment)
    {
        var document = await templates.GetShortcodeTemplatesDocumentAsync();
        var entry = document.ShortcodeTemplates.FirstOrDefault(entry => string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase));

        return entry.Key is null
            ? context.ApiNotFoundProblem()
            : TypedResults.Ok(await deployment.ExportRecipeAsync(new AllShortcodeTemplatesDeploymentStep { Names = [entry.Key] }, "ShortcodeTemplate-" + entry.Key, context.User));
    }
}
