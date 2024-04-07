using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Services;

public sealed class PasswordResetIdentityOptionsConfigurations : IConfigureOptions<IdentityOptions>
{
    private readonly TokenOptions _tokenOptions;

    public PasswordResetIdentityOptionsConfigurations(IOptions<TokenOptions> tokenOptions)
    {
        _tokenOptions = tokenOptions.Value;
    }

    public void Configure(IdentityOptions options)
    {
        options.Tokens.ProviderMap[_tokenOptions.PasswordResetTokenProvider] = new TokenProviderDescriptor(typeof(PasswordResetTokenProvider));
    }
}
