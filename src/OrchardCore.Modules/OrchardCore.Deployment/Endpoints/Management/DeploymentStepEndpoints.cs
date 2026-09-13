using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Deployment.Services;

namespace OrchardCore.Deployment.Endpoints.Management;

internal static class DeploymentStepEndpoints
{
    public static void AddDeploymentStepEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/deployment/plans/steps", ListAsync), "ApiListDeploymentSteps", "list", "Lists ordered step identities and safe configuration.")
            .Produces<IReadOnlyList<DeploymentStepResponse>>();
        Configure(routes.MapGet("api/deployment/plans/steps/by-id", GetAsync), "ApiGetDeploymentStep", "show", "Shows a step without write-only configuration.", true)
            .Produces<DeploymentStepResponse>();
        Configure(routes.MapPost("api/deployment/plans/steps", AddAsync), "ApiAddDeploymentStep", "add", "Adds a typed step using a caller-selected ID for retries.")
            .Produces<DeploymentStepWriteResponse>();
        Configure(routes.MapPut("api/deployment/plans/steps/by-id", UpdateAsync), "ApiUpdateDeploymentStep", "update", "Patches a typed step while retaining identity and order.", true)
            .Produces<DeploymentStepWriteResponse>();
        Configure(routes.MapDelete("api/deployment/plans/steps/by-id", DeleteAsync), "ApiDeleteDeploymentStep", "delete", "Deletes a step; an already absent step is unchanged.", true)
            .Produces<DeploymentStepWriteResponse>();
        Configure(routes.MapPut("api/deployment/plans/steps/order", OrderAsync), "ApiOrderDeploymentSteps", "order", "Reorders all steps after validating the complete identity list.")
            .Produces<DeploymentStepWriteResponse>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary, bool stepId = false) =>
        DeploymentPlanEndpoints.Configure(builder, name, verb, summary, "planId", group: ["deployment", "plans", "steps"], secondArgument: stepId ? "stepId" : null)
            .ProducesProblem(404).ProducesProblem(409).ProducesProblem(501);

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromServices] DeploymentStepRegistry registry, [FromQuery] long planId)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (planId <= 0) { return Invalid(); }
        var plan = await plans.GetAsync(planId);
        return plan is null ? context.ApiNotFoundProblem() : TypedResults.Ok<IReadOnlyList<DeploymentStepResponse>>(
            plan.DeploymentSteps.Select((step, position) => Describe(registry, step, position)).ToArray());
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromServices] DeploymentStepRegistry registry, [FromQuery] long planId, [FromQuery] string stepId)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (planId <= 0 || string.IsNullOrWhiteSpace(stepId)) { return Invalid(); }
        var plan = await plans.GetAsync(planId);
        var index = Find(plan, stepId);
        return index < 0 ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(registry, plan.DeploymentSteps[index], index));
    }

    internal static async Task<IResult> AddAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromServices] DeploymentStepRegistry registry, [FromQuery] long planId, [FromBody] DeploymentStepCreateRequest request)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (planId <= 0 || string.IsNullOrWhiteSpace(request?.Id) || request.Id.Length > 128 || string.IsNullOrWhiteSpace(request.Type) || request.Values is null) { return Invalid(); }
        var plan = await plans.GetAsync(planId);
        if (plan is null || !registry.IsAvailable(request.Type)) { return context.ApiNotFoundProblem(); }
        var index = Find(plan, request.Id);
        var existing = index >= 0 ? plan.DeploymentSteps[index] : null;
        if (existing is not null && registry.Resolve(existing) != request.Type) { return Conflict(); }
        var candidate = existing is null ? registry.Create(request.Type) : plans.CloneStep(existing);
        var definition = candidate is null ? null : registry.Definition(candidate);
        if (definition is null) { return Unsupported(); }
        candidate.Id = existing?.Id ?? request.Id;
        var errors = await definition.UpdateAsync(candidate, request.Values);
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors.ToDictionary(pair => pair.Key, pair => pair.Value)); }
        if (existing is not null)
        {
            return plans.StepEquals(existing, candidate)
                ? TypedResults.Ok(new DeploymentStepWriteResponse { Step = Describe(registry, existing, index) }) : Conflict();
        }
        var result = await plans.AddStepsAsync(planId, [candidate]);
        return result.Error == DeploymentStepManagementError.None
            ? TypedResults.Ok(new DeploymentStepWriteResponse { Changed = result.Changed, Step = Describe(registry, candidate, plan.DeploymentSteps.Count - 1) })
            : Failure(result.Error);
    }

    internal static async Task<IResult> UpdateAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromServices] DeploymentStepRegistry registry, [FromQuery] long planId, [FromQuery] string stepId, [FromBody] DeploymentStepUpdateRequest request)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (planId <= 0 || string.IsNullOrWhiteSpace(stepId) || request?.Values is null) { return Invalid(); }
        var plan = await plans.GetAsync(planId);
        var index = Find(plan, stepId);
        if (index < 0) { return context.ApiNotFoundProblem(); }
        var definition = registry.Definition(plan.DeploymentSteps[index]);
        if (definition is null) { return Unsupported(); }
        var candidate = plans.CloneStep(plan.DeploymentSteps[index]);
        var errors = await definition.UpdateAsync(candidate, request.Values);
        if (errors.Count > 0) { return TypedResults.ValidationProblem(errors.ToDictionary(pair => pair.Key, pair => pair.Value)); }
        var result = await plans.UpdateStepAsync(planId, candidate);
        return result.Error == DeploymentStepManagementError.None
            ? TypedResults.Ok(new DeploymentStepWriteResponse { Changed = result.Changed, Step = Describe(registry, candidate, index) })
            : Failure(result.Error);
    }

    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromQuery] long planId, [FromQuery] string stepId)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (planId <= 0 || string.IsNullOrWhiteSpace(stepId)) { return Invalid(); }
        var plan = await plans.GetAsync(planId);
        if (plan is null) { return context.ApiNotFoundProblem(); }
        if (Find(plan, stepId) < 0) { return TypedResults.Ok(new DeploymentStepWriteResponse()); }
        var result = await plans.DeleteStepAsync(planId, stepId);
        return result.Error == DeploymentStepManagementError.None ? TypedResults.Ok(new DeploymentStepWriteResponse { Changed = result.Changed }) : Failure(result.Error);
    }

    internal static async Task<IResult> OrderAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromQuery] long planId, [FromBody] DeploymentStepOrderRequest request)
    {
        if (!await DeploymentPlanEndpoints.AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (planId <= 0 || request?.StepIds is null) { return Invalid(); }
        var result = await plans.ReorderStepsAsync(planId, request.StepIds);
        return result.Error == DeploymentStepManagementError.None ? TypedResults.Ok(new DeploymentStepWriteResponse { Changed = result.Changed }) : Failure(result.Error);
    }

    private static int Find(DeploymentPlan plan, string id) => plan?.DeploymentSteps.FindIndex(step => string.Equals(step.Id, id, StringComparison.OrdinalIgnoreCase)) ?? -1;
    private static DeploymentStepResponse Describe(DeploymentStepRegistry registry, DeploymentStep step, int position)
    {
        var definition = registry.Definition(step);
        return new() { Id = step.Id, Type = registry.Resolve(step), Position = position, CanConfigure = definition is not null, Values = definition?.Describe(step) };
    }
    private static ProblemHttpResult Invalid() => TypedResults.Problem("Valid plan/step identifiers and a configuration object are required.", statusCode: 400);
    private static ProblemHttpResult Conflict() => TypedResults.Problem("The step identity is already used by a different definition.", statusCode: 409);
    private static ProblemHttpResult Unsupported() => TypedResults.Problem("This step has no enabled explicit configuration contract.", statusCode: 501);
    private static ProblemHttpResult Failure(DeploymentStepManagementError error) => TypedResults.Problem("The step mutation was rejected.",
        statusCode: error == DeploymentStepManagementError.NotFound ? 404 : 400);
}
