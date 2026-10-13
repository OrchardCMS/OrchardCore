using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;

namespace OrchardCore.Media.Services;

/// <summary>
/// Builds the "MediaApi" authorization policy, that requires an authenticated user against the "MediaApi"
/// scheme, which forwards to the scheme selected in the <see cref="MediaApiSettings"/>, see
/// <see cref="MediaApiAuthenticationOptionsConfiguration"/>, and the "MediaHub" policy, that accepts both the
/// admin cookie and the bearer "Api" scheme. Per-endpoint permission checks (MediaPermissions) still run in the
/// handlers.
/// </summary>
public sealed class MediaApiAuthorizationOptionsConfiguration : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(MediaApiConstants.AuthorizationPolicyName, policy =>
        {
            policy.AddAuthenticationSchemes(MediaApiConstants.AuthenticationSchemeName);
            policy.RequireAuthenticatedUser();
        });

        options.AddPolicy(MediaApiConstants.HubAuthorizationPolicyName, policy =>
        {
            policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api, IdentityConstants.ApplicationScheme);
            policy.Requirements.Add(new PermissionRequirement(MediaPermissions.ManageMedia));
            policy.RequireAuthenticatedUser();
        });
    }
}
