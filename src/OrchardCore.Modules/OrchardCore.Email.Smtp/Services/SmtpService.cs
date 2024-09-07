using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell.Builders;

namespace OrchardCore.Email.Services;

[Obsolete("Use IEmailService and its implementations instead")]
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

        return SmtpResult.Failed(result.Errors.SelectMany(x => x.Value).ToArray());
    }

    private IEmailProvider GetSmtpProvider()
    {
        var type = _emailProviderOptions.Providers.Where(x => x.Key.Contains("SMTP"))
            .OrderBy(entry => !entry.Value.IsEnabled ? 0 : 1)
            .ThenBy(entry => entry.Key != DefaultSmtpEmailProvider.TechnicalName ? 0 : 1)
            .Select(x => x.Value)
            .LastOrDefault()?.Type;

        if (type is not null)
        {
            return _serviceProvider.CreateInstance<IEmailProvider>(type);
        }

        return null;
    }
}
