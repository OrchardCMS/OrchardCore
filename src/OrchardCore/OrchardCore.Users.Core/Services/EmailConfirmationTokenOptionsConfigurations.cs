using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class EmailConfirmationTokenOptionsConfigurations : IConfigureOptions<TokenOptions>
{
    private readonly EmailConfirmationTokenProviderOptions _options;

    public EmailConfirmationTokenOptionsConfigurations(IOptions<EmailConfirmationTokenProviderOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(TokenOptions options)
    {
        options.EmailConfirmationTokenProvider = _options.Name;
    }
}
