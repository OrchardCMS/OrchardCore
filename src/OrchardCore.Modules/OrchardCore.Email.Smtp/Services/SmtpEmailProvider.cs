using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpEmailProvider : SmtpEmailProviderBase<SmtpOptions>, IEmailProviderDescription
{
    public const string TechnicalName = "SMTP";

    public SmtpEmailProvider(
        IOptionsMonitor<SmtpOptions> options,
        IEmailAddressValidator emailAddressValidator,
        ILogger<SmtpEmailProvider> logger,
        IStringLocalizer<SmtpEmailProvider> stringLocalizer)
        : base(options, emailAddressValidator, logger, stringLocalizer)
    {
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<SmtpEmailProvider>("Simple Mail Transfer Protocol (SMTP)");
}
