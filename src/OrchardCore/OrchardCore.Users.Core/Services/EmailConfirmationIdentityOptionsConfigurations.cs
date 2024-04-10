using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Services;

public sealed class EmailConfirmationIdentityOptionsConfigurations : IConfigureOptions<IdentityOptions>
{
    private readonly TokenOptions _tokenOptions;

    public EmailConfirmationIdentityOptionsConfigurations(IOptions<TokenOptions> tokenOptions)
    {
        _tokenOptions = tokenOptions.Value;
    }

    public void Configure(IdentityOptions options)
    {
        options.Tokens.ProviderMap[_tokenOptions.EmailConfirmationTokenProvider] = new TokenProviderDescriptor(typeof(EmailConfirmationTokenProvider));
    }
}
