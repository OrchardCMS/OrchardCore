using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Settings;

namespace OrchardCore.Email.Services;

public class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly ISiteService _siteService;

    public SmtpProviderOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(SmtpEmailProvider));

        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
        var settings = site.As<SmtpSettings>();

        typeOptions.IsEnabled = settings.IsEnabled ?? settings.HasValidSettings();

        options.TryAddProvider(SmtpEmailProvider.TechnicalName, typeOptions);
    }
}
