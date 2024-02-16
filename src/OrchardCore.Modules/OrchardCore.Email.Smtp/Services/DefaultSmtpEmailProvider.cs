using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Smtp.Services;

public class DefaultSmtpEmailProvider : SmtpEmailProviderBase, IEmailProvider
{
    public const string TechnicalName = "DefaultSMTP";

    public DefaultSmtpEmailProvider(
        IOptions<DefaultSmtpOptions> options,
        ILogger<DefaultSmtpEmailProvider> logger,
        IStringLocalizer<DefaultSmtpEmailProvider> stringLocalizer)
        : base(options.Value, logger, stringLocalizer)
    {
    }

    public LocalizedString DisplayName => S["Simple Mail Transfer Protocol (Default SMTP)"];
}
