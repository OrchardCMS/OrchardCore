using Microsoft.Extensions.Options;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Smtp.Services;

public sealed class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly IOptionsMonitor<SmtpOptions> _smtpOptions;
    private readonly DefaultSmtpOptions _defaultSmtpOptions;

    public SmtpProviderOptionsConfigurations(
        IOptionsMonitor<SmtpOptions> smtpOptions,
        IOptions<DefaultSmtpOptions> defaultSmtpOptions)
    {
        _smtpOptions = smtpOptions;
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
            IsEnabled = _smtpOptions.CurrentValue.IsEnabled,
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
