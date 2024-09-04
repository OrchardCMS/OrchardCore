using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Smtp.Services;

public class DefaultSmtpEmailProvider : SmtpEmailProviderBase
{
    public const string TechnicalName = "DefaultSMTP";

    public DefaultSmtpEmailProvider(
        IOptions<DefaultSmtpOptions> options,
        IEmailAddressValidator emailAddressValidator,
        ILogger<DefaultSmtpEmailProvider> logger,
        IStringLocalizer<DefaultSmtpEmailProvider> stringLocalizer)
        : base(options.Value, emailAddressValidator, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName => S["Simple Mail Transfer Protocol (Default SMTP)"];
}
