using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Localization;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailProvider : AzureEmailProviderBase<AzureEmailOptions>, IEmailProviderDescription
{
    public const string TechnicalName = "Azure";

    public AzureEmailProvider(
        IOptionsMonitor<AzureEmailOptions> options,
        ILogger<AzureEmailProvider> logger,
        IStringLocalizer<AzureEmailProvider> stringLocalizer)
        : base(options, logger, stringLocalizer)
    {
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<AzureEmailProvider>("Azure Communication Services");
}
