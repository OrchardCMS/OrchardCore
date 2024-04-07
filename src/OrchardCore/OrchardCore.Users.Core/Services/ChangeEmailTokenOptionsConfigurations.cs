using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ChangeEmailTokenOptionsConfigurations : IConfigureOptions<TokenOptions>
{
    private readonly ChangeEmailTokenProviderOptions _options;

    public ChangeEmailTokenOptionsConfigurations(IOptions<ChangeEmailTokenProviderOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(TokenOptions options)
    {
        options.ChangeEmailTokenProvider = _options.Name;
    }
}
