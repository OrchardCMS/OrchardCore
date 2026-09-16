using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment.Artifacts;
using OrchardCore.Deployment.Operations;
using OrchardCore.Deployment.Steps;
using OrchardCore.Json;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Deployment.Endpoints.Management;

internal static class DeploymentOperationEndpoints
{
    public static void AddDeploymentOperationEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapPost("api/deployment/operations/export", ExportAsync), "ApiQueueDeploymentExport", "export",
            "Queues a deployment plan snapshot for export.").Produces<DeploymentOperationResponse>(202);
        Configure(routes.MapPost("api/deployment/operations/import", ImportAsync), "ApiQueueDeploymentImport", "import",
            "Queues an owned uploaded artifact for import; recipe execution may partially commit.").Produces<DeploymentOperationResponse>(202);
        Configure(routes.MapGet("api/deployment/operations/{id}", GetAsync), "ApiGetDeploymentOperation", "show",
            "Shows the caller's deployment operation state.").Produces<DeploymentOperationResponse>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary)
    {
        var metadata = new CliOperationMetadata(["deployment", "operations"], verb)
        {
            Capability = "deployment-plans", RequiresConfirmation = verb == "import",
        };
        if (verb == "show") { metadata.Arguments.Add(new CliArgumentMetadata("id", 0)); }
        return builder.WithName(name).WithSummary(summary).WithTags("Deployment Management").WithCliCommand(metadata)
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(409);
    }

    internal static async Task<IResult> ExportAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IDeploymentPlanService plans, [FromServices] DeploymentOperationStore operations,
        [FromServices] IOptions<DocumentJsonSerializerOptions> options, [FromBody] DeploymentExportRequest request)
    {
        var owner = await OwnerAsync(context, authorization, DeploymentPermissions.Export);
        if (owner is null) { return context.ApiForbidProblem(); }
        if (!ValidRequestId(request.RequestId) || request.PlanId <= 0) { return InvalidRequest(); }
        var plan = await plans.GetAsync(request.PlanId);
        if (plan is null) { return context.ApiNotFoundProblem(); }
        if (plan.DeploymentSteps.Any(step => step is UnknownDeploymentStep))
        {
            return TypedResults.Problem("The plan contains unavailable deployment steps.", statusCode: 409);
        }
        return await AcceptAsync(context, operations, owner, request.RequestId, DeploymentOperationKind.Export,
            JsonSerializer.Serialize(plan, options.Value.SerializerOptions));
    }

    internal static async Task<IResult> ImportAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] DeploymentArtifactStore artifacts, [FromServices] DeploymentOperationStore operations,
        [FromBody] DeploymentImportRequest request)
    {
        var owner = await OwnerAsync(context, authorization, DeploymentPermissions.Import);
        if (owner is null) { return context.ApiForbidProblem(); }
        if (!ValidRequestId(request.RequestId) || string.IsNullOrWhiteSpace(request.ArtifactId)) { return InvalidRequest(); }
        // An accepted request survives artifact expiry/deletion. Return its durable
        // outcome on a retry without admitting another execution.
        if (await operations.FindRequestAsync(owner, request.RequestId, context.RequestAborted) is not null)
        {
            return await AcceptAsync(context, operations, owner, request.RequestId, DeploymentOperationKind.Import, request.ArtifactId);
        }
        var artifact = await artifacts.FindAsync(request.ArtifactId, owner);
        if (artifact is null || artifact.Kind != DeploymentArtifactKind.Import) { return context.ApiNotFoundProblem(); }
        return await AcceptAsync(context, operations, owner, request.RequestId, DeploymentOperationKind.Import, artifact.Id);
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] DeploymentOperationStore operations, string id)
    {
        if (!await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement)) { return context.ApiForbidProblem(); }
        var owner = DeploymentArtifactOwner.Get(context.User);
        if (owner is null) { return context.ApiForbidProblem(); }
        var operation = await operations.FindAsync(id, owner, context.RequestAborted);
        if (operation is null) { return context.ApiNotFoundProblem(); }
        if (!await authorization.AuthorizeAsync(context.User,
            operation.Kind == DeploymentOperationKind.Export ? DeploymentPermissions.Export : DeploymentPermissions.Import))
        {
            return context.ApiForbidProblem();
        }
        return TypedResults.Ok(Describe(operation));
    }

    private static async Task<IResult> AcceptAsync(HttpContext context, DeploymentOperationStore operations, string owner,
        string requestId, DeploymentOperationKind kind, string payload)
    {
        try
        {
            var operation = await operations.CreateAsync(owner, requestId, kind, payload, context.RequestAborted, DeploymentExecutionIdentity.Capture(context.User));
            return TypedResults.Accepted(context.Request.PathBase + "/api/deployment/operations/" + operation.Id, Describe(operation));
        }
        catch (DeploymentOperationBusyException)
        {
            return TypedResults.Problem("The request is being accepted. Retry with the same request ID.", statusCode: 409);
        }
        catch (DeploymentRequestConflictException)
        {
            return TypedResults.Problem("The request ID already belongs to a different deployment request.", statusCode: 409);
        }
    }

    private static bool ValidRequestId(string id) => !string.IsNullOrWhiteSpace(id) && id.Length <= 128;
    private static ProblemHttpResult InvalidRequest() => TypedResults.Problem("Provide a request ID of 1 to 128 characters and a valid target.", statusCode: 400);
    private static async Task<string> OwnerAsync(HttpContext context, IAuthorizationService authorization, Permission permission) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement)
            && await authorization.AuthorizeAsync(context.User, permission) ? DeploymentArtifactOwner.Get(context.User) : null;
    private static DeploymentOperationResponse Describe(DeploymentOperation operation) => new()
    {
        Id = operation.Id, Kind = operation.Kind.ToString().ToLowerInvariant(), State = operation.State.ToString().ToLowerInvariant(),
        ArtifactId = operation.ArtifactId, ErrorCode = operation.ErrorCode, CreatedUtc = operation.CreatedUtc, UpdatedUtc = operation.UpdatedUtc,
    };
}

/// <summary>Requests a snapshot export using a caller-generated idempotency key.</summary>
public sealed class DeploymentExportRequest
{
    /// <summary>Gets or sets the caller-generated request ID; reuse it only for an identical request.</summary>
    public string RequestId { get; set; }
    /// <summary>Gets or sets the tenant-local deployment plan ID.</summary>
    public long PlanId { get; set; }
}

/// <summary>Requests execution of a previously validated import artifact.</summary>
public sealed class DeploymentImportRequest
{
    /// <summary>Gets or sets the caller-generated request ID; retries do not replay execution.</summary>
    public string RequestId { get; set; }
    /// <summary>Gets or sets the caller-owned import artifact ID.</summary>
    public string ArtifactId { get; set; }
}

/// <summary>Safe deployment operation metadata without payload or owner details.</summary>
public sealed class DeploymentOperationResponse
{
    /// <summary>Gets the tenant-local operation ID.</summary>
    public string Id { get; init; }
    /// <summary>Gets the export or import operation kind.</summary>
    public string Kind { get; init; }
    /// <summary>Gets pending, running, succeeded, failed or uncertain state.</summary>
    public string State { get; init; }
    /// <summary>Gets the generated artifact ID for a completed export.</summary>
    public string ArtifactId { get; init; }
    /// <summary>Gets a stable diagnostic code when execution did not succeed.</summary>
    public string ErrorCode { get; init; }
    /// <summary>Gets acceptance time in UTC.</summary>
    public DateTime CreatedUtc { get; init; }
    /// <summary>Gets the last state transition time in UTC.</summary>
    public DateTime UpdatedUtc { get; init; }
}
