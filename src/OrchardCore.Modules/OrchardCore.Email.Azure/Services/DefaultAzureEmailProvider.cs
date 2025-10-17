using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Core;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public class DefaultAzureEmailProvider : AzureEmailProviderBase
{
    public const string TechnicalName = "DefaultAzure";

    public DefaultAzureEmailProvider(
        IOptions<DefaultAzureEmailOptions> options,
        IOptionsMonitor<AzureOptions> optionsMonitor,
        ILogger<DefaultAzureEmailProvider> logger,
        IStringLocalizer<DefaultAzureEmailProvider> stringLocalizer)
        : base(options.Value, optionsMonitor, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName
        => S["Default Azure Communication Services"];
}
