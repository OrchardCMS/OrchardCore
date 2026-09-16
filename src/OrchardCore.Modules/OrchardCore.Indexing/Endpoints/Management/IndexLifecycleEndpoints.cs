using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Operations;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.Indexing.Endpoints.Management;

internal static class IndexLifecycleEndpoints
{
    internal static void AddIndexLifecycleEndpoints(this IEndpointRouteBuilder routes)
    {
        foreach (var action in Enum.GetValues<IndexLifecycleAction>())
        {
            var command = action.ToString().ToLowerInvariant();
            var metadata = new CliOperationMetadata(["indexes"], command)
            {
                Capability = IndexDiscoveryEndpoints.CapabilityName,
                RequiresConfirmation = action != IndexLifecycleAction.Synchronize,
            };
            metadata.Arguments.Add(new CliArgumentMetadata("id", 0));
            Configure(routes.MapPost("api/indexes/by-id:" + command, (HttpContext context,
                [FromServices] IAuthorizationService authorization, [FromServices] IIndexProfileManager profiles,
                [FromServices] IndexOperationRunner operations, [FromServices] IOptions<IndexLifecycleOptions> options,
                [FromQuery] string id) => QueueAsync(context, authorization, profiles, operations, options, id, action)))
                .WithName("Api" + action + "IndexOperation").WithSummary("Queues index " + command + "; inspect the returned operation for completion.")
                .WithCliCommand(metadata).Produces<IndexOperationResponse>(202).ProducesProblem(501);
        }
        var show = new CliOperationMetadata(["indexes", "operations"], "show") { Capability = IndexDiscoveryEndpoints.CapabilityName };
        show.Arguments.Add(new CliArgumentMetadata("id", 0));
        Configure(routes.MapGet("api/indexes/operations/by-id", GetAsync))
            .WithName("ApiGetIndexOperation").WithSummary("Shows the persisted status of an index operation.")
            .WithCliCommand(show).Produces<IndexOperationResponse>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder route) => route.WithTags("Indexes").DisableAntiforgery()
        .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement), new PermissionRequirement(IndexingPermissions.ManageIndexes)))
        .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);

    internal static async Task<IResult> QueueAsync(HttpContext context, IAuthorizationService authorization,
        IIndexProfileManager profiles, IndexOperationRunner operations, IOptions<IndexLifecycleOptions> options,
        string id, IndexLifecycleAction action)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        var profile = await profiles.FindByIdAsync(id);
        if (profile is null) { return context.ApiNotFoundProblem(); }
        if (profile.Type != IndexingConstants.ContentsIndexSource || !options.Value.RemoteProviders.Contains(profile.ProviderName))
        {
            return TypedResults.Problem("Remote lifecycle operations are not enabled for this provider and source.", statusCode: 501);
        }
        var operation = await operations.QueueAsync(id, action);
        return TypedResults.Accepted($"{context.Request.PathBase}/api/indexes/operations/by-id?id={operation.OperationId}", Describe(operation));
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IndexOperationRunner operations, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An operation identifier is required.", statusCode: 400); }
        var operation = await operations.FindAsync(id);
        return operation is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(operation));
    }

    private static IndexOperationResponse Describe(IndexOperation operation) => new()
    {
        Id = operation.OperationId, IndexId = operation.IndexId, Action = operation.Action, State = operation.State,
        CreatedUtc = operation.CreatedUtc, UpdatedUtc = operation.UpdatedUtc, Outcome = operation.Outcome, LastTaskId = operation.LastTaskId,
    };

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, IndexingPermissions.ManageIndexes);
}
