using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents.Deployment.Download;

internal static class ContentExportEndpoints
{
    internal static void Map(IEndpointRouteBuilder routes)
    {
        var metadata = new CliOperationMetadata(["content"], "export");
        metadata.Arguments.Add(new CliArgumentMetadata("contentItemId", 0));
        routes.MapGet("api/content/{contentItemId}/export", ExportAsync)
            .WithName("ApiExportContentItemJson").WithTags("Content").WithSummary("Exports published or latest content using the admin download permissions and JSON format.")
            .WithCliCommand(metadata).RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement), new PermissionRequirement(DeploymentPermissions.Export)))
            .Produces<JsonObject>().ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
    }

    private static async Task<IResult> ExportAsync(string contentItemId, HttpContext context, [FromServices] ContentExportService exports, [FromQuery] bool latest = false)
    {
        try
        {
            var item = await exports.GetAsync(contentItemId, latest, context.User);
            return item is null ? TypedResults.NotFound() : TypedResults.Ok(ContentExportService.Serialize(item));
        }
        catch (UnauthorizedAccessException) { return context.ApiForbidProblem(); }
    }
}
