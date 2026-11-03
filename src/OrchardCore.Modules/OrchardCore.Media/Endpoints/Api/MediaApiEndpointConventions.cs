using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

internal static class MediaApiEndpointConventions
{
    public const string CapabilityName = "media";
    public const string TagName = "Media";

    public static RouteHandlerBuilder MapLegacyGet(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapGet(pattern, handler)
            .WithTags("MediaApi")
            .ExcludeFromDescription()
            .DisableAntiforgery()
            .RequireAuthorization(MediaApiConstants.AuthorizationPolicyName);

    public static RouteHandlerBuilder MapLegacyPost(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapPost(pattern, handler)
            .WithTags("MediaApi")
            .ExcludeFromDescription()
            .DisableAntiforgery()
            .RequireAuthorization(MediaApiConstants.AuthorizationPolicyName);

    public static RouteHandlerBuilder MapManagementGet(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapManagementGroup(TagName).MapGet(pattern, handler);

    public static RouteHandlerBuilder MapManagementPost(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapManagementGroup(TagName).MapPost(pattern, handler);

    public static RouteHandlerBuilder MapManagementPut(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapManagementGroup(TagName).MapPut(pattern, handler);

    public static RouteHandlerBuilder MapManagementDelete(this IEndpointRouteBuilder builder, string pattern, Delegate handler)
        => builder.MapManagementGroup(TagName).MapDelete(pattern, handler);
}
