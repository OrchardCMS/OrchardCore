using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public sealed class EmailOptionsConfiguration : IConfigureOptions<EmailOptions>
{
    private readonly ISiteService _siteService;
    private readonly IOptionsMonitor<EmailProviderOptions> _emailProviderOptions;

    public EmailOptionsConfiguration(
        ISiteService siteService,
        IOptionsMonitor<EmailProviderOptions> emailProviderOptions)
    {
        _siteService = siteService;
        _emailProviderOptions = emailProviderOptions;
    }

    public void Configure(EmailOptions options)
    {
        var emailSettings = _siteService.GetSettings<EmailSettings>();
        var emailProviderOptions = _emailProviderOptions.CurrentValue;

        if (!string.IsNullOrEmpty(emailSettings.DefaultProviderName)
            && emailProviderOptions.Providers.TryGetValue(emailSettings.DefaultProviderName, out var provider)
            && provider.IsEnabled)
        {
            options.DefaultProviderName = emailSettings.DefaultProviderName;

            return;
        }

        if (emailProviderOptions.Providers.Count > 0)
        {
            options.DefaultProviderName = emailProviderOptions.Providers
                .Where(x => x.Value.IsEnabled)
                .Select(x => x.Key)
                .LastOrDefault()
                ?? emailProviderOptions.Providers.Keys.Last();
        }
    }
}
