using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment.Services;

namespace OrchardCore.Deployment.Endpoints.Management;

internal static class DeploymentStepTypeEndpoints
{
    public static void AddDeploymentStepTypeEndpoints(this IEndpointRouteBuilder routes)
    {
        DeploymentPlanEndpoints.Configure(routes.MapGet("api/deployment/step-types", ListAsync), "ApiListDeploymentStepTypes", "list",
            "Lists registered step factories and explicit configuration support.", resource: "step-types")
            .Produces<IReadOnlyList<DeploymentStepTypeDescriptor>>();
        DeploymentPlanEndpoints.Configure(routes.MapGet("api/deployment/step-types/schema", SchemaAsync), "ApiGetDeploymentStepSchema", "schema",
            "Returns the explicit patch schema for a registered deployment step factory.", "type", "step-types")
            .Produces<JsonObject>().ProducesProblem(404).ProducesProblem(501);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] DeploymentStepRegistry registry)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        return TypedResults.Ok(registry.List());
    }

    internal static async Task<IResult> SchemaAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] DeploymentStepRegistry registry, [FromQuery] string type)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(type)) { return TypedResults.Problem("A step factory type is required.", statusCode: 400); }
        if (!registry.IsAvailable(type)) { return context.ApiNotFoundProblem(); }
        var schema = registry.GetSchema(type);
        return schema is null
            ? TypedResults.Problem("This step factory has no explicit remote configuration contract.", statusCode: 501)
            : TypedResults.Ok(schema);
    }
}
