using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public class DefaultAzureEmailProvider : AzureEmailProviderBase<DefaultAzureEmailOptions>
{
    public const string TechnicalName = "DefaultAzure";

    public DefaultAzureEmailProvider(
        IOptionsMonitor<DefaultAzureEmailOptions> options,
        ILogger<DefaultAzureEmailProvider> logger,
        IStringLocalizer<DefaultAzureEmailProvider> stringLocalizer)
        : base(options, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName
        => S["Default Azure Communication Services"];
}
