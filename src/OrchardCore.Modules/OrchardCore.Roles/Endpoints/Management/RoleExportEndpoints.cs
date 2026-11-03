using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.RemoteManagement;
using OrchardCore.Roles.Deployment;
using OrchardCore.Security;

namespace OrchardCore.Roles.Endpoints.Management;

internal static class RoleExportEndpoints
{
    internal static void AddRoleExportEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup(RoleManagementApiEndpointConventions.TagName, DeploymentPermissions.Export, RolesPermissions.ManageRoles)
            .MapGet("api/roles/{roleId}:export", ExportAsync)
            .WithName("ApiExportRole")
            .WithSummary("Exports a role as a recipe.")
            .WithDescription("Returns a recipe with one step, built by the deployment source of the all-items step.")
            .WithCliCommand(new CliOperationMetadata(["roles"], "export")
            {
                Capability = RoleManagementApiEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("roleId", 0) },
            })
            .Produces<JsonObject>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAsync(string roleId, HttpContext context, [FromServices] RoleManager<IRole> roles, [FromServices] IDeploymentManager deployment)
    {
        return await roles.FindByIdAsync(roleId) is Role role
            ? TypedResults.Ok(await deployment.ExportRecipeAsync(new AllRolesDeploymentStep { Names = [role.RoleName] }, "Role-" + role.RoleName, context.User))
            : context.ApiNotFoundProblem();
    }
}
