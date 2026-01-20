using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ChangeEmailIdentityOptionsConfigurations : IConfigureOptions<IdentityOptions>
{
    private readonly ChangeEmailTokenProviderOptions _tokenOptions;

    public ChangeEmailIdentityOptionsConfigurations(IOptions<ChangeEmailTokenProviderOptions> tokenOptions)
    {
        _tokenOptions = tokenOptions.Value;
    }

    public void Configure(IdentityOptions options)
    {
        options.Tokens.ChangeEmailTokenProvider = _tokenOptions.Name;
        options.Tokens.ProviderMap[_tokenOptions.Name] = new TokenProviderDescriptor(typeof(ChangeEmailTokenProvider));
    }
}
