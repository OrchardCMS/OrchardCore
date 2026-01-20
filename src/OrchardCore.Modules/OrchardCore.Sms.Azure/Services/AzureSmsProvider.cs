using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class AzureSmsProvider : AzureSmsProviderBase
{
    public const string TechnicalName = "Azure";

    public AzureSmsProvider(
        IOptions<AzureSmsOptions> options,
        IPhoneFormatValidator phoneFormatValidator,
        ILogger<AzureSmsProvider> logger,
        IStringLocalizer<AzureSmsProvider> stringLocalizer)
        : base(options.Value, phoneFormatValidator, logger, stringLocalizer)
    {
    }

    public override LocalizedString Name
        => S["Azure Communication Services"];
}
