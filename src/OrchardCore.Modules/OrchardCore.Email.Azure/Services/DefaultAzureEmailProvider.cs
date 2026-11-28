using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Localization;

namespace OrchardCore.Email.Azure.Services;

public class DefaultAzureEmailProvider : AzureEmailProviderBase<DefaultAzureEmailOptions>, IEmailProviderDescription
{
    public const string TechnicalName = "DefaultAzure";

    public DefaultAzureEmailProvider(
        IOptionsMonitor<DefaultAzureEmailOptions> options,
        ILogger<DefaultAzureEmailProvider> logger,
        IStringLocalizer<DefaultAzureEmailProvider> stringLocalizer)
        : base(options, logger, stringLocalizer)
    {
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<DefaultAzureEmailProvider>("Default Azure Communication Services");
}
