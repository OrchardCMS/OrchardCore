using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Media.Services;

/// <summary>
/// Builds the "MediaHub" authorization policy that accepts both the admin cookie and the bearer "Api" scheme.
/// The "MediaApi" policy depends on the <see cref="MediaApiSettings"/> and is built on each request by the
/// <see cref="MediaApiAuthorizationPolicyProvider"/>, so that it applies without restarting the tenant.
/// </summary>
public sealed class MediaApiAuthorizationOptionsConfiguration : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(MediaApiConstants.HubAuthorizationPolicyName, policy =>
        {
            policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api, IdentityConstants.ApplicationScheme);
            policy.Requirements.Add(new PermissionRequirement(MediaPermissions.ManageMedia));
            policy.RequireAuthenticatedUser();
        });
    }
}
