using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Models;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;

namespace OrchardCore.Indexing.Endpoints.Management;

internal static class IndexDiscoveryEndpoints
{
    internal const string CapabilityName = "indexes";

    public static void AddIndexDiscoveryEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/indexes", ListAsync), "ApiListIndexes", "Lists stored index identities without private provider configuration.",
            new CliOperationMetadata(["indexes"], "list") { Capability = CapabilityName })
            .Produces<IndexListResponse>().ProducesProblem(400);
        Configure(routes.MapGet("api/indexes/by-id", GetAsync), "ApiGetIndex", "Shows a stored index identity by its administrative identifier.",
            new CliOperationMetadata(["indexes"], "show") { Capability = CapabilityName, Arguments = { new CliArgumentMetadata("id", 0) } })
            .Produces<IndexProfileResponse>().ProducesProblem(400).ProducesProblem(404);
        Configure(routes.MapGet("api/indexes/providers", ProvidersAsync), "ApiListIndexProviders", "Lists registered index providers and source types.",
            new CliOperationMetadata(["indexes", "providers"], "list") { Capability = CapabilityName })
            .Produces<IReadOnlyList<IndexProviderResponse>>();
    }

    private static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string name, string summary, CliOperationMetadata cli) =>
        builder.WithName(name).WithTags("Indexes").WithSummary(summary).WithCliCommand(cli)
            .DisableAntiforgery().RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api).RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(IndexingPermissions.ManageIndexes)))
            .ProducesProblem(401).ProducesProblem(403);

    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager manager, [AsParameters] IndexListRequest request)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var skip = request.Skip ?? 0;
        var take = request.Take ?? 50;
        if (context.ApiPagingProblem(skip, take) is { } pagingError) { return pagingError; }
        if (skip > int.MaxValue - (2L * take)) { return TypedResults.Problem("The offset must fit a 32-bit integer.", statusCode: 400); }
        var query = new QueryContext { Name = request.Search, Sorted = true };
        var result = await manager.PageAsync((skip / take) + 1, take, query);
        var items = result.Models;
        // The store reads whole pages, so an offset between two of them spans both.
        if (skip % take != 0)
        {
            var next = await manager.PageAsync((skip / take) + 2, take, query);
            items = items.Concat(next.Models).Skip(skip % take).Take(take).ToArray();
        }
        return TypedResults.Ok(new IndexListResponse
        {
            Skip = skip, Take = take, TotalCount = result.Count,
            Items = items.Select(Describe).ToArray(),
        });
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IIndexProfileManager manager, [FromQuery] string id)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        if (string.IsNullOrWhiteSpace(id)) { return TypedResults.Problem("An index identifier is required.", statusCode: 400); }
        var profile = await manager.FindByIdAsync(id);
        return profile is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(profile));
    }

    internal static async Task<IResult> ProvidersAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] IOptions<IndexingOptions> options, [FromServices] IOptions<IndexLifecycleOptions> lifecycle)
    {
        if (!await AuthorizedAsync(context, authorization)) { return context.ApiForbidProblem(); }
        var value = options.Value;
        return TypedResults.Ok<IReadOnlyList<IndexProviderResponse>>(value.Providers.Values
            .OrderBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .Select(provider => new IndexProviderResponse
            {
                Name = provider.ProviderName, DisplayName = provider.DisplayName?.Value ?? provider.ProviderName,
                Sources = value.Sources.Values.Where(source => string.Equals(source.ProviderName, provider.ProviderName, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(source => source.Type, StringComparer.OrdinalIgnoreCase)
                    .Select(source => new IndexSourceResponse { Type = source.Type, DisplayName = source.DisplayName?.Value ?? source.Type, Description = source.Description?.Value,
                        LifecycleActions = source.Type == IndexingConstants.ContentsIndexSource && lifecycle.Value.RemoteProviders.Contains(provider.ProviderName)
                            ? ["synchronize", "reset", "rebuild"] : [],
                    }).ToArray(),
            }).ToArray());
    }

    private static IndexProfileResponse Describe(IndexProfile profile) => new()
    {
        Id = profile.Id, Name = profile.Name, IndexName = profile.IndexName,
        ProviderName = profile.ProviderName, Type = profile.Type, CreatedUtc = profile.CreatedUtc,
    };

    private static async Task<bool> AuthorizedAsync(HttpContext context, IAuthorizationService authorization) =>
        await authorization.AuthorizeAsync(context.User, RemoteManagementPermissions.AccessRemoteManagement) &&
        await authorization.AuthorizeAsync(context.User, IndexingPermissions.ManageIndexes);
}
