using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Email.Services;

[Obsolete]
public class SmtpService : ISmtpService
{
    private readonly EmailProviderOptions _emailProviderOptions;
    private readonly IServiceProvider _serviceProvider;

    protected readonly IStringLocalizer S;

    public SmtpService(
        IOptions<EmailProviderOptions> emailProviderOptions,
        IServiceProvider serviceProvider,
        IStringLocalizer<SmtpService> stringLocalizer)
    {
        _emailProviderOptions = emailProviderOptions.Value;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public async Task<SmtpResult> SendAsync(MailMessage message)
    {
        var provider = GetSmtpProvider();

        if (provider == null)
        {
            return SmtpResult.Failed([S["Unable to find any SMTP providers."]]);
        }

        var result = await provider.SendAsync(message);

        if (result.Succeeded)
        {
            return SmtpResult.Success;
        }

        return SmtpResult.Failed(result.Errors.ToArray());
    }

    private IEmailProvider GetSmtpProvider()
    {
        IEmailProvider provider = null;

        if (_emailProviderOptions.Providers.TryGetValue(DefaultSmtpEmailProvider.TechnicalName, out var defaultSmtpProvider)
            && defaultSmtpProvider.IsEnabled)
        {
            provider = _serviceProvider.CreateInstance<IEmailProvider>(defaultSmtpProvider.Type);
        }

        if (provider == null)
        {
            if (_emailProviderOptions.Providers.TryGetValue(SmtpEmailProvider.TechnicalName, out var smtpProvider)
                && smtpProvider.IsEnabled)
            {
                provider = _serviceProvider.CreateInstance<IEmailProvider>(smtpProvider.Type);
            }

            if (provider is null && defaultSmtpProvider is not null)
            {
                provider = _serviceProvider.CreateInstance<IEmailProvider>(defaultSmtpProvider.Type);
            }
            else if (provider is null && smtpProvider is not null)
            {
                provider = _serviceProvider.CreateInstance<IEmailProvider>(smtpProvider.Type);
            }
        }

        return provider;
    }
}
