using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Endpoints.Management;

internal static class DeploymentPlanEndpoints
{
    public static void AddDeploymentPlanEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/deployment/plans", ListAsync), "ApiListDeploymentPlans", "list",
            "Lists deployment plans without step configuration.")
            .Produces<DeploymentPlanListResponse>();
        Configure(routes.MapGet("api/deployment/plans/by-id", GetAsync), "ApiGetDeploymentPlan", "show",
            "Shows a deployment plan without embedded step data.", "id")
            .Produces<DeploymentPlanResponse>().ProducesProblem(404);
        Configure(routes.MapPost("api/deployment/plans", CreateAsync), "ApiCreateDeploymentPlan", "create",
            "Creates an empty plan; retrying an existing name leaves its steps unchanged.")
            .Produces<DeploymentPlanWriteResponse>().ProducesProblem(409);
        Configure(routes.MapPut("api/deployment/plans/by-id", UpdateAsync), "ApiUpdateDeploymentPlan", "update",
            "Renames a plan while preserving all steps and their order.", "id")
            .Produces<DeploymentPlanWriteResponse>().ProducesProblem(404).ProducesProblem(409);
        Configure(routes.MapDelete("api/deployment/plans/by-id", DeleteAsync), "ApiDeleteDeploymentPlan", "delete",
            "Deletes a plan; an already absent plan is unchanged.", "id")
            .Produces<DeploymentPlanWriteResponse>();
    }

    internal static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary, string argument = null, string resource = "plans", string[] group = null, string secondArgument = null)
    {
        var metadata = new CliOperationMetadata(group ?? ["deployment", resource], verb)
        {
            Capability = "deployment-plans",
            RequiresConfirmation = verb == "delete",
        };
        if (argument is not null)
        {
            metadata.Arguments.Add(new CliArgumentMetadata(argument, 0));
        }
        if (secondArgument is not null) { metadata.Arguments.Add(new CliArgumentMetadata(secondArgument, 1)); }
        return builder.WithName(name).WithTags("Deployment Management").WithSummary(summary).WithCliCommand(metadata)
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(DeploymentPermissions.ManageDeploymentPlan)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403);
    }

    internal static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, DeploymentPermissions.ManageDeploymentPlan);

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [AsParameters] DeploymentPlanListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (skip < 0 || take is < 1 or > 200)
        {
            return TypedResults.Problem("Skip must be nonnegative and take must be between 1 and 200.", statusCode: 400);
        }
        var page = await plans.ListAsync(request.Search, skip, take);
        return TypedResults.Ok(new DeploymentPlanListResponse
        {
            Skip = skip, Take = take, TotalCount = page.TotalCount, Items = page.Items.Select(Describe).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromQuery] long id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (id <= 0) { return InvalidId(); }
        var plan = await plans.GetAsync(id);
        return plan is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(plan));
    }

    internal static async Task<IResult> CreateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromBody] DeploymentPlanNameRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(request?.Name)) { return InvalidName(); }
        var existing = await plans.FindByNameAsync(request.Name);
        if (existing is not null)
        {
            return TypedResults.Ok(new DeploymentPlanWriteResponse { Plan = Describe(existing) });
        }
        return Result(await plans.CreateAsync(request.Name));
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromQuery] long id, [FromBody] DeploymentPlanNameRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (id <= 0) { return InvalidId(); }
        if (string.IsNullOrWhiteSpace(request?.Name)) { return InvalidName(); }
        return Result(await plans.RenameAsync(id, request.Name));
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromQuery] long id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (id <= 0) { return InvalidId(); }
        return TypedResults.Ok(new DeploymentPlanWriteResponse { Changed = await plans.DeleteAsync(id) });
    }

    private static IResult Result(DeploymentPlanManagementResult result) => result.Error switch
    {
        DeploymentPlanManagementError.None => TypedResults.Ok(new DeploymentPlanWriteResponse
        {
            Changed = result.Changed, Plan = Describe(result.Plan),
        }),
        DeploymentPlanManagementError.MissingName => InvalidName(),
        DeploymentPlanManagementError.DuplicateName => TypedResults.Problem("A deployment plan with the same name already exists.", statusCode: 409),
        _ => TypedResults.Problem("The deployment plan does not exist.", statusCode: 404),
    };

    private static ProblemHttpResult InvalidName() => TypedResults.Problem("A nonempty plan name is required.", statusCode: 400);
    private static ProblemHttpResult InvalidId() => TypedResults.Problem("A positive plan identifier is required.", statusCode: 400);
    private static DeploymentPlanResponse Describe(DeploymentPlan plan) => new()
    {
        Id = plan.Id, Name = plan.Name, StepCount = plan.DeploymentSteps.Count,
    };
}
