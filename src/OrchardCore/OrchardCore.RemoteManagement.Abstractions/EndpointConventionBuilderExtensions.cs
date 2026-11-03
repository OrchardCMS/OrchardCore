using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.RemoteManagement;

/// <summary>
/// Provides endpoint conventions for remote management operations.
/// </summary>
public static class EndpointConventionBuilderExtensions
{
    /// <summary>
    /// Creates a group for management endpoints, which authenticate with the API scheme
    /// and require the permission to access remote management.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    /// <param name="tag">The OpenAPI tag of the endpoints.</param>
    /// <param name="permissions">The other permissions all the endpoints of the group require.</param>
    /// <returns>The route group builder.</returns>
    public static RouteGroupBuilder MapManagementGroup(this IEndpointRouteBuilder routes, string tag, params Permission[] permissions)
    {
        ArgumentNullException.ThrowIfNull(routes);

        return routes.MapGroup(string.Empty)
            .WithTags(tag)
            .DisableAntiforgery()
            .RequireAuthorization(policy => policy
                .AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api)
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(RemoteManagementPermissions.AccessRemoteManagement))
                .AddRequirements(permissions.Select(permission => new PermissionRequirement(permission)).ToArray()));
    }

    /// <summary>
    /// Adds CLI projection metadata to an API endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The endpoint convention builder type.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="metadata">The CLI operation metadata.</param>
    /// <returns>The endpoint convention builder.</returns>
    public static TBuilder WithCliCommand<TBuilder>(this TBuilder builder, CliOperationMetadata metadata)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(metadata);

        builder.Add(endpointBuilder => endpointBuilder.Metadata.Add(metadata));

        return builder;
    }

    /// <summary>
    /// Declares in the OpenAPI document that the responses of the endpoint return the version of the resource in the <c>ETag</c> header.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder.</returns>
    public static RouteHandlerBuilder ProducesETag(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithMetadata(new ResourceVersionMetadata());
    }

    /// <summary>
    /// Declares in the OpenAPI document that the endpoint returns the version of the resource in the <c>ETag</c> header,
    /// accepts the optional <c>If-Match</c> header, and answers 412 when it doesn't match.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder.</returns>
    public static RouteHandlerBuilder AcceptsIfMatch(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithMetadata(new ResourceVersionMetadata { AcceptsIfMatch = true })
            .ProducesProblem(StatusCodes.Status412PreconditionFailed);
    }
}
