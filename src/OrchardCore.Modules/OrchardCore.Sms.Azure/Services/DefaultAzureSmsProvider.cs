using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure.Services;

public sealed class DefaultAzureSmsProvider : AzureSmsProviderBase<DefaultAzureSmsOptions>
{
    public const string TechnicalName = "DefaultAzure";

    public DefaultAzureSmsProvider(
        IOptionsMonitor<DefaultAzureSmsOptions> options,
        IPhoneFormatValidator phoneFormatValidator,
        ILogger<DefaultAzureSmsProvider> logger,
        IStringLocalizer<DefaultAzureSmsProvider> stringLocalizer)
        : base(options, phoneFormatValidator, logger, stringLocalizer)
    {
    }

    public override LocalizedString Name
        => S["Default Azure Communication Services"];
}
