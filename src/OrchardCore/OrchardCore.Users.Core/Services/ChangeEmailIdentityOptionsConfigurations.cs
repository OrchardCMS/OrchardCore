using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users.Services;

public sealed class ChangeEmailIdentityOptionsConfigurations : IConfigureOptions<IdentityOptions>
{
    private readonly TokenOptions _tokenOptions;

    public ChangeEmailIdentityOptionsConfigurations(IOptions<TokenOptions> tokenOptions)
    {
        _tokenOptions = tokenOptions.Value;
    }

    public void Configure(IdentityOptions options)
    {
        options.Tokens.ProviderMap.TryAdd(_tokenOptions.ChangeEmailTokenProvider, new TokenProviderDescriptor(typeof(ChangeEmailTokenProvider)));
    }
}
