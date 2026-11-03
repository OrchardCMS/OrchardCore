using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Services;
using OrchardCore.Queries.Deployment;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Queries.Endpoints.Api;

internal static class QueryExportEndpoints
{
    internal static void AddQueryExportEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup(QueryManagementEndpointConventions.TagName, DeploymentPermissions.Export, QueryPermissions.ManageQueries)
            .MapGet("api/queries/named/{name}:export", ExportAsync)
            .WithName("ApiExportQuery")
            .WithSummary("Exports a named query as a recipe.")
            .WithDescription("Returns a recipe with one step, built by the deployment source of the all-items step.")
            .WithCliCommand(new CliOperationMetadata(["queries"], "export")
            {
                Capability = QueryManagementEndpointConventions.CapabilityName,
                Arguments = { new CliArgumentMetadata("name", 0) },
            })
            .Produces<JsonObject>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAsync(string name, HttpContext context, [FromServices] IQueryManager queries, [FromServices] IDeploymentManager deployment)
    {
        var query = await queries.GetQueryAsync(name);

        return query is null
            ? context.ApiNotFoundProblem()
            : TypedResults.Ok(await deployment.ExportRecipeAsync(new AllQueriesDeploymentStep { Names = [query.Name] }, "Query-" + query.Name, context.User));
    }
}
