using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Configuration;

public sealed class EmailOptionsConfiguration : IConfigureOptions<EmailOptions>
{
    private readonly ISiteService _siteService;

    public EmailOptionsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(EmailOptions options)
    {
        var emailSettings = _siteService.GetSettings<EmailSettings>();

        if (!string.IsNullOrEmpty(emailSettings.DefaultProviderName))
        {
            options.DefaultProviderName = emailSettings.DefaultProviderName;
        }
    }
}
