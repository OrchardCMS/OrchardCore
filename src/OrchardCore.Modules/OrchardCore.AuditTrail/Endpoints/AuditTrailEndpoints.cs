using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.AuditTrail.Endpoints;

internal static class AuditTrailEndpoints
{
    public static void AddAuditTrailEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/audit-trail/events", ListAsync), "ApiListAuditTrailEvents", "list", "Searches audit metadata with the same filters as the administration UI.")
            .Produces<AuditTrailPage>();
        Configure(routes.MapGet("api/audit-trail/events/by-id", GetAsync), "ApiGetAuditTrailEvent", "show", "Shows audit event metadata without stored event payloads.", true)
            .Produces<AuditTrailEventResponse>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string verb, string summary, bool argument = false)
    {
        var metadata = new CliOperationMetadata(["audit-trail", "events"], verb) { Capability = "audit-trail" };
        if (argument) { metadata.Arguments.Add(new CliArgumentMetadata("eventId", 0)); }
        return builder.WithName(name).WithTags("Audit Trail").WithSummary(summary).WithCliCommand(metadata)
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(
                    new OrchardCore.Security.PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new OrchardCore.Security.PermissionRequirement(AuditTrailPermissions.ViewAuditTrail)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404);
    }

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IAuditTrailAdminListFilterParser parser, [FromServices] IAuditTrailAdminListQueryService queries,
        [FromQuery] string q, [FromQuery] int? skip, [FromQuery] int? take)
    {
        if (!await authorization.AuthorizeAsync(context.User, AuditTrailPermissions.ViewAuditTrail)) { return context.ApiForbidProblem(); }
        var selectedSkip = skip ?? 0;
        var selectedTake = take ?? 50;
        if (context.ApiPagingProblem(selectedSkip, selectedTake) is { } pagingError) { return pagingError; }
        if (selectedSkip > int.MaxValue - (2L * selectedTake) || q?.Length > 4096)
        {
            return TypedResults.Problem("The offset must fit a 32-bit integer, and q must have at most 4096 characters.", statusCode: 400);
        }
        var options = new AuditTrailIndexOptions { FilterResult = parser.Parse(q ?? string.Empty) };
        var result = await queries.QueryAsync((selectedSkip / selectedTake) + 1, selectedTake, options);
        var events = result.Events;
        // The query service reads whole pages, so an offset between two of them spans both.
        if (selectedSkip % selectedTake != 0)
        {
            var next = await queries.QueryAsync((selectedSkip / selectedTake) + 2, selectedTake, options);
            events = events.Concat(next.Events).Skip(selectedSkip % selectedTake).Take(selectedTake).ToArray();
        }
        return TypedResults.Ok(new AuditTrailPage
        {
            Skip = selectedSkip, Take = selectedTake, TotalCount = result.TotalCount,
            Items = events.Select(Describe).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IAuditTrailManager manager, [FromQuery] string eventId)
    {
        if (!await authorization.AuthorizeAsync(context.User, AuditTrailPermissions.ViewAuditTrail)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(eventId) || eventId.Length > 256) { return TypedResults.Problem("A valid event ID is required.", statusCode: 400); }
        var item = await manager.GetEventAsync(eventId);
        return item is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(item));
    }

    private static AuditTrailEventResponse Describe(AuditTrailEvent item) => new()
    {
        EventId = item.EventId, Name = item.Name, Category = item.Category, CorrelationId = item.CorrelationId,
        UserId = item.UserId, UserName = item.UserName, CreatedUtc = item.CreatedUtc,
    };
}

/// <summary>A bounded page of tenant audit metadata.</summary>
public sealed class AuditTrailPage
{
    /// <summary>Gets the applied zero-based offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the maximum number of events returned.</summary>
    public int Take { get; init; }
    /// <summary>Gets the total count matching the filters.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the event metadata, excluding arbitrary recorded payloads.</summary>
    public IReadOnlyList<AuditTrailEventResponse> Items { get; init; }
}

/// <summary>Safe audit metadata; event properties, content snapshots and client IP addresses are not projected.</summary>
public sealed class AuditTrailEventResponse
{
    /// <summary>Gets the stable event identifier.</summary>
    public string EventId { get; init; }
    /// <summary>Gets the event name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the event category.</summary>
    public string Category { get; init; }
    /// <summary>Gets the correlation identifier linking related events.</summary>
    public string CorrelationId { get; init; }
    /// <summary>Gets the recorded actor identifier.</summary>
    public string UserId { get; init; }
    /// <summary>Gets the recorded actor name.</summary>
    public string UserName { get; init; }
    /// <summary>Gets the UTC event time.</summary>
    public DateTime CreatedUtc { get; init; }
}
