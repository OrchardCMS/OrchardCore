using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Settings;

namespace OrchardCore.Security.Services;

/// <summary>
/// Applies the security headers configured by the tenant <see cref="SecuritySettings"/>, and rebuilds
/// the header policies whenever these settings are updated.
/// </summary>
internal sealed class SecuritySettingsHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<SecuritySettings> _securitySettings;

    private HeadersPolicy _policy;

    public SecuritySettingsHeadersMiddleware(RequestDelegate next, IOptionsMonitor<SecuritySettings> securitySettings)
    {
        _next = next;
        _securitySettings = securitySettings;
    }

    public Task Invoke(HttpContext context)
    {
        foreach (var provider in GetOptions().HeaderPolicyProviders)
        {
            provider.ApplyPolicy(context);
        }

        return _next.Invoke(context);
    }

    private SecurityHeadersOptions GetOptions()
    {
        var settings = _securitySettings.CurrentValue;

        var policy = _policy;
        if (ReferenceEquals(policy?.Settings, settings))
        {
            return policy.Options;
        }

        var options = new SecurityHeadersOptions()
            .AddContentSecurityPolicy(settings.ContentSecurityPolicy)
            .AddContentTypeOptions()
            .AddPermissionsPolicy(settings.PermissionsPolicy)
            .AddReferrerPolicy(settings.ReferrerPolicy);

        foreach (var provider in options.HeaderPolicyProviders)
        {
            provider.InitializePolicy();
        }

        // The policy is swapped as a whole so that concurrent requests never observe a partially initialized one.
        _policy = new HeadersPolicy(settings, options);

        return options;
    }

    private sealed record HeadersPolicy(SecuritySettings Settings, SecurityHeadersOptions Options);
}
