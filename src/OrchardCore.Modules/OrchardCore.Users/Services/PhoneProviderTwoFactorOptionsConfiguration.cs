using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class PhoneProviderTwoFactorOptionsConfiguration : IConfigureOptions<TwoFactorOptions>
{
    private readonly IdentityOptions _identityOptions;

    public PhoneProviderTwoFactorOptionsConfiguration(IOptions<IdentityOptions> identityOptions)
    {
        _identityOptions = identityOptions.Value;
    }

    public void Configure(TwoFactorOptions options)
    {
        if (string.IsNullOrEmpty(_identityOptions.Tokens.ChangePhoneNumberTokenProvider) ||
            options.Providers.Contains(_identityOptions.Tokens.ChangePhoneNumberTokenProvider))
        {
            return;
        }

        options.Providers.Add(_identityOptions.Tokens.ChangePhoneNumberTokenProvider);
    }
}
