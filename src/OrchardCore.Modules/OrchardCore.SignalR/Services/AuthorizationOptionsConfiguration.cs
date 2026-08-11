using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace OrchardCore.SignalR.Services;

internal sealed class AuthorizationOptionsConfiguration : IPostConfigureOptions<AuthorizationOptions>
{
    private readonly AuthenticationOptions _authenticationOptions;

    public AuthorizationOptionsConfiguration(IOptions<AuthenticationOptions> authenticationOptions)
    {
        _authenticationOptions = authenticationOptions.Value;
    }

    public void PostConfigure(string name, AuthorizationOptions options)
    {
        var defaultAuthenticateScheme = _authenticationOptions.DefaultAuthenticateScheme ?? _authenticationOptions.DefaultScheme;

        options.AddPolicy("SignalR", policy =>
        {
            if (!string.IsNullOrEmpty(defaultAuthenticateScheme))
            {
                policy.AddAuthenticationSchemes(defaultAuthenticateScheme);
            }

            if (!string.Equals(defaultAuthenticateScheme, OrchardCoreConstants.AuthenticationSchemes.Api, StringComparison.Ordinal))
            {
                policy.AddAuthenticationSchemes(OrchardCoreConstants.AuthenticationSchemes.Api);
            }

            policy.RequireAuthenticatedUser();
        });
    }
}
