using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpProviderOptionsConfigurations : IConfigureOptions<EmailProviderOptions>
{
    private readonly SmtpOptions _smtpOptions;

    public SmtpProviderOptionsConfigurations(IOptions<SmtpOptions> smtpOptions)
    {
        _smtpOptions = smtpOptions.Value;
    }

    public void Configure(EmailProviderOptions options)
    {
        var typeOptions = new EmailProviderTypeOptions(typeof(SmtpEmailProvider))
        {
            IsEnabled = _smtpOptions.IsEnabled
        };

        options.TryAddProvider(SmtpEmailProvider.TechnicalName, typeOptions);
    }
}
