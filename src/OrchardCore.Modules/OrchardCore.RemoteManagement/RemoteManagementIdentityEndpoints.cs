using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OpenIddict.Abstractions;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;

namespace OrchardCore.RemoteManagement;

internal static class RemoteManagementIdentityEndpoints
{
    internal const string UserIdentity = "user";
    internal const string ClientIdentity = "client";

    public static IEndpointRouteBuilder AddRemoteManagementIdentityEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapManagementGroup("RemoteManagement")
            .MapGet("api/management/me", GetAsync)
            .WithName("ApiGetRemoteManagementIdentity")
            .WithSummary("Gets the identity of the caller.")
            .WithDescription("Returns the tenant, the user or client behind the access token, its scopes, and the permissions of the tenant it effectively has.")
            .WithCliCommand(new CliOperationMetadata(["management"], "whoami")
            {
                Capability = "remote-management",
            })
            .Produces<RemoteManagementIdentityResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    internal static async Task<IResult> GetAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IPermissionService permissionService,
        [FromServices] ShellSettings shellSettings)
    {
        // The response depends on the token, so a shared cache must not serve it to another caller.
        httpContext.Response.Headers.CacheControl = "no-store";

        var user = httpContext.User;
        var subject = user.FindFirstValue(OpenIddictConstants.Claims.Subject) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        var clientId = user.GetPresenters().FirstOrDefault() ?? user.GetClaim(OpenIddictConstants.Claims.ClientId);

        // A client authenticating for itself is its own subject.
        var isClient = !string.IsNullOrEmpty(clientId) && (subject is null || string.Equals(subject, clientId, StringComparison.Ordinal));

        var permissions = new List<string>();
        foreach (var permission in (await permissionService.GetPermissionsAsync()).DistinctBy(permission => permission.Name, StringComparer.Ordinal))
        {
            if (await authorizationService.AuthorizeAsync(user, permission))
            {
                permissions.Add(permission.Name);
            }
        }

        permissions.Sort(StringComparer.Ordinal);

        return TypedResults.Ok(new RemoteManagementIdentityResponse
        {
            Tenant = shellSettings.Name,
            Type = isClient ? ClientIdentity : UserIdentity,
            Subject = subject,
            Name = user.Identity?.Name,
            ClientId = clientId,
            Scopes = user.GetScopes().Order(StringComparer.Ordinal).ToArray(),
            Permissions = permissions.ToArray(),
        });
    }
}

/// <summary>
/// Describes the caller of the management APIs.
/// </summary>
internal sealed class RemoteManagementIdentityResponse
{
    /// <summary>The name of the tenant the token is used on.</summary>
    public string Tenant { get; init; }

    /// <summary>Whether the token represents a <c>user</c> or a <c>client</c> authenticating for itself.</summary>
    public string Type { get; init; }

    /// <summary>The identifier of the user or client.</summary>
    public string Subject { get; init; }

    /// <summary>The name of the user or client, when the token has one.</summary>
    public string Name { get; init; }

    /// <summary>The identifier of the OpenID application the token was issued to, when known.</summary>
    public string ClientId { get; init; }

    /// <summary>The scopes of the token.</summary>
    public string[] Scopes { get; init; } = [];

    /// <summary>The names of the permissions of the tenant that the caller effectively has with this token.</summary>
    public string[] Permissions { get; init; } = [];
}
