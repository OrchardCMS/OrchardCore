using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;

namespace OrchardCore.Email.Smtp.Services;

public sealed class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly SmtpOptions _smtpOptions;
    private readonly DefaultSmtpOptions _defaultSmtpOptions;

    public SmtpProviderOptionsConfigurations(
        IOptions<SmtpOptions> smtpOptions,
        IOptions<DefaultSmtpOptions> defaultSmtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
        _defaultSmtpOptions = defaultSmtpOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        ConfigureTenantProvider(options);

        if (_defaultSmtpOptions.IsEnabled)
        {
            ConfigureDefaultProvider(options);
        }
    }

    private void ConfigureTenantProvider(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(SmtpEmailProvider))
        {
            IsEnabled = _smtpOptions.IsEnabled,
        };

        options.TryAddProvider(SmtpEmailProvider.TechnicalName, typeOptions);
    }

    private static void ConfigureDefaultProvider(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(DefaultSmtpEmailProvider))
        {
            IsEnabled = true,
        };

        options.TryAddProvider(DefaultSmtpEmailProvider.TechnicalName, typeOptions);
    }
}
