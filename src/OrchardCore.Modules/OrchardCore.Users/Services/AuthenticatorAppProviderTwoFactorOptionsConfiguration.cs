using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class AuthenticatorAppProviderTwoFactorOptionsConfiguration : IConfigureOptions<TwoFactorOptions>
{
    private readonly IdentityOptions _identityOptions;

    public AuthenticatorAppProviderTwoFactorOptionsConfiguration(IOptions<IdentityOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;
    }

    public void Configure(TwoFactorOptions options)
    {
        if (string.IsNullOrEmpty(_identityOptions.Tokens.AuthenticatorTokenProvider) ||
            options.Providers.Contains(_identityOptions.Tokens.AuthenticatorTokenProvider))
        {
            return;
        }

        options.Providers.Add(_identityOptions.Tokens.AuthenticatorTokenProvider);
    }
}
