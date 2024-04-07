using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class PasswordResetTokenOptionsConfigurations : IConfigureOptions<TokenOptions>
{
    private readonly PasswordResetTokenProviderOptions _options;

    public PasswordResetTokenOptionsConfigurations(IOptions<PasswordResetTokenProviderOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(TokenOptions options)
    {
        options.PasswordResetTokenProvider = _options.Name;
    }
}
