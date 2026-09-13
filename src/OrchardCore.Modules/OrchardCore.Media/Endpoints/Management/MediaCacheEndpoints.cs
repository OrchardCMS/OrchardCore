using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Endpoints.Management;

internal static class MediaCacheEndpoints
{
    public static void AddMediaCacheEndpoints(this IEndpointRouteBuilder routes)
    {
        MediaProfileEndpoints.Configure(routes.MapGet("api/media/cache", GetAsync), "ApiGetMediaCache", "show", "Shows configured tenant media caches.",
            group: ["media", "cache"], permission: MediaPermissions.ManageAssetCache).Produces<MediaCacheResponse>();
        MediaProfileEndpoints.Configure(routes.MapPost("api/media/cache/purge", PurgeAsync), "ApiPurgeMediaCache", "purge", "Purges either resized images or the configured remote asset cache in this tenant.",
            "cache", group: ["media", "cache"], permission: MediaPermissions.ManageAssetCache).Produces(204).ProducesProblem(503).ProducesProblem(500);
    }

    internal static async Task<IResult> GetAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaCacheManagementService service)
    {
        if (!await authorization.AuthorizeAsync(context.User, MediaPermissions.ManageAssetCache)) { return context.ApiForbidProblem(); }
        return TypedResults.Ok(new MediaCacheResponse { RemoteConfigured = service.RemoteConfigured, ResizedConfigured = service.ResizedConfigured });
    }

    internal static async Task<IResult> PurgeAsync(HttpContext context, [FromServices] IAuthorizationService authorization,
        [FromServices] MediaCacheManagementService service, [FromQuery] string cache)
    {
        if (!await authorization.AuthorizeAsync(context.User, MediaPermissions.ManageAssetCache)) { return context.ApiForbidProblem(); }
        return await service.PurgeAsync(cache, context.RequestAborted) switch
        {
            MediaCachePurgeStatus.Purged => TypedResults.NoContent(),
            MediaCachePurgeStatus.Unavailable => TypedResults.Problem("The selected tenant cache is not configured.", statusCode: 503),
            MediaCachePurgeStatus.Invalid => TypedResults.Problem("Select resized or remote.", statusCode: 400),
            _ => TypedResults.Problem("The cache provider could not complete the purge.", statusCode: 500),
        };
    }
}

/// <summary>Reports which tenant media cache providers are configured, without exposing storage paths or credentials.</summary>
public sealed class MediaCacheResponse
{
    /// <summary>Gets whether a remote asset cache is configured.</summary>
    public bool RemoteConfigured { get; init; }
    /// <summary>Gets whether a resized image cache is configured.</summary>
    public bool ResizedConfigured { get; init; }
}
