using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpEmailProvider : SmtpEmailProviderBase
{
    public const string TechnicalName = "SMTP";

    public SmtpEmailProvider(
        IOptions<SmtpOptions> options,
        IEmailAddressValidator emailAddressValidator,
        ILogger<SmtpEmailProvider> logger,
        IStringLocalizer<SmtpEmailProvider> stringLocalizer)
        : base(options.Value, emailAddressValidator, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName => S["Simple Mail Transfer Protocol (SMTP)"];
}
