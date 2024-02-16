using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;

namespace OrchardCore.Email.Smtp.Services;

public class DefaultSmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly DefaultSmtpOptions _defaultSmtpOptions;

    public DefaultSmtpProviderOptionsConfigurations(IOptions<DefaultSmtpOptions> defaultSmtpOptions)
    {
        _defaultSmtpOptions = defaultSmtpOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        if (!_defaultSmtpOptions.ConfigurationExists())
        {
            return;
        }

        // At this point, we should enable the default SMTP provider.
        var typeOptions = new EmailProviderTypeOptions(typeof(DefaultSmtpEmailProvider))
        {
            IsEnabled = true,
        };

        options.TryAddProvider(DefaultSmtpEmailProvider.TechnicalName, typeOptions);
    }
}
