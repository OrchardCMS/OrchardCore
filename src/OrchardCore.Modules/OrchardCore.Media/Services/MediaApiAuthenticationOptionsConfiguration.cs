using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Media.Services;

/// <summary>
/// Registers the "MediaApi" authentication scheme required by the "MediaApi" authorization policy. It forwards
/// to the scheme selected in the current <see cref="MediaApiSettings"/>, read on each request: the admin cookie
/// by default, or the bearer "Api" scheme when configured. So the policy doesn't depend on the settings, and
/// a change of the selected scheme applies without restarting the tenant.
/// </summary>
public sealed class MediaApiAuthenticationOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<PolicySchemeOptions>
{
    public void Configure(AuthenticationOptions options)
        => options.AddScheme<PolicySchemeHandler>(MediaApiConstants.AuthenticationSchemeName, displayName: null);

    public void Configure(string name, PolicySchemeOptions options)
    {
        if (!string.Equals(name, MediaApiConstants.AuthenticationSchemeName, StringComparison.Ordinal))
        {
            return;
        }

        options.ForwardDefaultSelector = GetSelectedScheme;
    }

    public void Configure(PolicySchemeOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

    private static string GetSelectedScheme(HttpContext context)
    {
        // The site settings are cached, so they can be read on each request.
        var settings = context.RequestServices.GetRequiredService<ISiteService>().GetSettings<MediaApiSettings>();

        return settings.AuthenticationScheme == MediaApiAuthenticationScheme.Bearer
            ? OrchardCoreConstants.AuthenticationSchemes.Api
            : IdentityConstants.ApplicationScheme;
    }
}
