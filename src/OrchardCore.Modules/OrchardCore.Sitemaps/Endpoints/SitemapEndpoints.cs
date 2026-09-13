using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Endpoints;

internal static class SitemapEndpoints
{
    public static void AddSitemapEndpoints(this IEndpointRouteBuilder routes)
    {
        Configure(routes.MapGet("api/sitemaps", ListAsync), "ApiListSitemaps", "list", "Lists tenant sitemaps and indexes.").Produces<SitemapResponse[]>();
        Configure(routes.MapGet("api/sitemaps/{id}", GetAsync), "ApiGetSitemap", "show", "Shows a sitemap definition.", "id").Produces<SitemapResponse>();
        Configure(routes.MapPost("api/sitemaps", CreateAsync), "ApiCreateSitemap", "create", "Creates a sitemap or index. Read back the server-generated ID before retrying.", input: true).Produces<SitemapResponse>();
        Configure(routes.MapPut("api/sitemaps/{id}", UpdateAsync), "ApiUpdateSitemap", "update", "Replaces a definition while retaining its sources.", "id", true).Produces<SitemapResponse>();
        Configure(routes.MapDelete("api/sitemaps/{id}", DeleteAsync), "ApiDeleteSitemap", "delete", "Deletes a sitemap; an absent ID is unchanged.", "id").Produces(204);
        Configure(routes.MapPost("api/sitemaps/{id}/enable", EnableAsync), "ApiEnableSitemap", "enable", "Enables public sitemap routing.", "id").Produces(204);
        Configure(routes.MapPost("api/sitemaps/{id}/disable", DisableAsync), "ApiDisableSitemap", "disable", "Disables public sitemap routing.", "id").Produces(204);
    }

    internal static RouteHandlerBuilder Configure(RouteHandlerBuilder builder, string operation, string verb, string summary,
        string argument = null, bool input = false, bool sources = false, string secondArgument = null)
    {
        var metadata = new CliOperationMetadata(sources ? ["sitemaps", "sources"] : ["sitemaps"], verb)
        {
            Capability = "sitemaps", InputMode = input ? CliInputMode.Json : CliInputMode.Options,
            RequiresConfirmation = verb == "delete",
        };
        if (argument is not null) { metadata.Arguments.Add(new CliArgumentMetadata(argument, 0)); }
        if (secondArgument is not null) { metadata.Arguments.Add(new CliArgumentMetadata(secondArgument, 1)); }
        return builder.WithName(operation).WithTags("Sitemaps").WithSummary(summary).WithCliCommand(metadata).DisableAntiforgery()
            .RequireAuthorization(policy => policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser().AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement),
                    new PermissionRequirement(SitemapsPermissions.ManageSitemaps)))
            .ProducesProblem(400).ProducesProblem(401).ProducesProblem(403).ProducesProblem(404).ProducesProblem(409);
    }

    internal static async Task<IResult> ListAsync([FromServices] ISitemapManager manager, [FromQuery] int? skip, [FromQuery] int? take)
    {
        if (skip < 0 || take < 1 || take > 200) { return TypedResults.Problem("Skip must be nonnegative and take between 1 and 200.", statusCode: 400); }
        return TypedResults.Ok((await manager.GetSitemapsAsync()).OrderBy(sitemap => sitemap.Name, StringComparer.OrdinalIgnoreCase)
            .Skip(skip ?? 0).Take(take ?? 50).Select(Describe).ToArray());
    }
    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] ISitemapManager manager, string id)
    {
        var sitemap = await manager.GetSitemapAsync(id);
        return sitemap is null ? context.ApiNotFoundProblem() : TypedResults.Ok(Describe(sitemap));
    }
    internal static Task<IResult> CreateAsync(HttpContext context, [FromServices] SitemapManagementService service, [FromBody] SitemapDefinition input)
        => SaveAsync(context, service, input, null);
    internal static Task<IResult> UpdateAsync(HttpContext context, [FromServices] SitemapManagementService service, string id, [FromBody] SitemapDefinition input)
        => SaveAsync(context, service, input, id);
    private static async Task<IResult> SaveAsync(HttpContext context, SitemapManagementService service, SitemapDefinition input, string id)
    {
        var result = await service.SaveAsync(input, id);
        if (result.NotFound) { return context.ApiNotFoundProblem(); }
        if (result.Errors.Count > 0) { return TypedResults.ValidationProblem(result.Errors); }
        return TypedResults.Ok(Describe(result.Sitemap));
    }
    internal static async Task<IResult> DeleteAsync([FromServices] ISitemapManager manager, string id)
    {
        if (await manager.LoadSitemapAsync(id) is not null) { await manager.DeleteSitemapAsync(id); }
        return TypedResults.NoContent();
    }
    internal static Task<IResult> EnableAsync(HttpContext context, [FromServices] ISitemapManager manager, string id) => StatusAsync(context, manager, id, true);
    internal static Task<IResult> DisableAsync(HttpContext context, [FromServices] ISitemapManager manager, string id) => StatusAsync(context, manager, id, false);
    private static async Task<IResult> StatusAsync(HttpContext context, ISitemapManager manager, string id, bool enabled)
    {
        var sitemap = await manager.LoadSitemapAsync(id);
        if (sitemap is null) { return context.ApiNotFoundProblem(); }
        if (SitemapMutations.SetEnabled(sitemap, enabled)) { await manager.UpdateSitemapAsync(sitemap); }
        return TypedResults.NoContent();
    }
    private static SitemapResponse Describe(SitemapType sitemap) => new()
    {
        Id = sitemap.SitemapId, Name = sitemap.Name, Path = sitemap.Path, Enabled = sitemap.Enabled,
        Kind = sitemap.GetType().Name, SourceCount = sitemap.SitemapSources.Count,
        ContainedSitemapIds = sitemap.SitemapSources.OfType<SitemapIndexSource>().SelectMany(source => source.ContainedSitemapIds).ToArray(),
    };
}

/// <summary>A safe tenant sitemap definition, excluding cache paths and arbitrary source properties.</summary>
public sealed class SitemapResponse
{
    /// <summary>Gets the server-generated sitemap identifier.</summary>
    public string Id { get; init; }
    /// <summary>Gets the administrative name.</summary>
    public string Name { get; init; }
    /// <summary>Gets the public relative XML path.</summary>
    public string Path { get; init; }
    /// <summary>Gets the sitemap kind.</summary>
    public string Kind { get; init; }
    /// <summary>Gets whether public routing is enabled.</summary>
    public bool Enabled { get; init; }
    /// <summary>Gets the number of sources.</summary>
    public int SourceCount { get; init; }
    /// <summary>Gets the regular sitemap IDs included by an index.</summary>
    public string[] ContainedSitemapIds { get; init; }
}
