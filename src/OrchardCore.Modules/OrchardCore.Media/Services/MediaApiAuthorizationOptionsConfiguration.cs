using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

/// <summary>
/// Builds the "MediaApi" authorization policy from <see cref="MediaApiSettings"/>: it requires an
/// authenticated user against exactly one scheme — the admin cookie by default, or the bearer "Api"
/// scheme when configured. Per-endpoint permission checks (MediaPermissions) still run in the
/// handlers. The policies are built once per shell; the settings driver requests a shell release
/// when the API scheme changes so they are rebuilt.
/// </summary>
public sealed class MediaApiAuthorizationOptionsConfiguration : IConfigureOptions<AuthorizationOptions>
{
    private readonly ISiteService _siteService;

    public MediaApiAuthorizationOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(AuthorizationOptions options)
    {
        var scheme = _siteService.GetSettings<MediaApiSettings>().AuthenticationScheme == MediaApiAuthenticationScheme.Bearer
            ? OrchardCoreConstants.AuthenticationSchemes.Api
            : IdentityConstants.ApplicationScheme;

        options.AddPolicy(MediaApiConstants.AuthorizationPolicyName, policy =>
        {
            policy.AddAuthenticationSchemes(scheme);
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
