using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Core.Services;

public sealed class EmailOptionsConfiguration : IConfigureOptions<EmailOptions>
{
    private readonly ISiteService _siteService;
    private readonly EmailProviderOptions _emailProviderOptions;

    public EmailOptionsConfiguration(
        ISiteService siteService,
        IOptions<EmailProviderOptions> emailProviderOptions)
    {
        _siteService = siteService;
        _emailProviderOptions = emailProviderOptions.Value;
    }

    public void Configure(EmailOptions options)
    {
        var emailSettings = _siteService.GetSettingsAsync<EmailSettings>()
            .GetAwaiter()
            .GetResult();

        if (!string.IsNullOrEmpty(emailSettings.DefaultProviderName)
            && _emailProviderOptions.Providers.TryGetValue(emailSettings.DefaultProviderName, out var provider)
            && provider.IsEnabled)
        {
            options.DefaultProviderName = emailSettings.DefaultProviderName;

            return;
        }

        if (_emailProviderOptions.Providers.Count > 0)
        {
            options.DefaultProviderName = _emailProviderOptions.Providers
                .Where(x => x.Value.IsEnabled)
                .Select(x => x.Key)
                .LastOrDefault()
                ?? _emailProviderOptions.Providers.Keys.Last();

            return;
        }
    }
}
