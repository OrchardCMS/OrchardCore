using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public class DefaultAzureEmailProvider : AzureEmailProviderBase
{
    public const string TechnicalName = "DefaultAzure";

    public DefaultAzureEmailProvider(
        IOptions<DefaultAzureEmailOptions> options,
        ILogger<DefaultAzureEmailProvider> logger,
        IStringLocalizer<DefaultAzureEmailProvider> stringLocalizer)
        : base(options.Value, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName
        => S["Default Azure Communication Services"];
}
