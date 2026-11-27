using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;

namespace OrchardCore.Email.Smtp.Services;

public class DefaultSmtpEmailProvider : SmtpEmailProviderBase<DefaultSmtpOptions>, IEmailProviderDescription
{
    public const string TechnicalName = "DefaultSMTP";

    public DefaultSmtpEmailProvider(
        IOptionsMonitor<DefaultSmtpOptions> options,
        IEmailAddressValidator emailAddressValidator,
        ILogger<DefaultSmtpEmailProvider> logger,
        IStringLocalizer<DefaultSmtpEmailProvider> stringLocalizer)
        : base(options, emailAddressValidator, logger, stringLocalizer)
    {
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<DefaultSmtpEmailProvider>("Simple Mail Transfer Protocol (Default SMTP)");
}
