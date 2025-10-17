using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Core;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailProvider : AzureEmailProviderBase
{
    public const string TechnicalName = "Azure";

    public AzureEmailProvider(
        IOptions<AzureEmailOptions> options,
        IOptionsMonitor<AzureOptions> optionsMonitor,
        ILogger<AzureEmailProvider> logger,
        IStringLocalizer<AzureEmailProvider> stringLocalizer)
        : base(options.Value, optionsMonitor, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName
        => S["Azure Communication Services"];
}
