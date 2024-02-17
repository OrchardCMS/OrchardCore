using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public class DefaultAzureEmailProvider : AzureEmailProviderBase, IEmailProvider
{
    public const string TechnicalName = "DefaultAzure";

    public DefaultAzureEmailProvider(
        IOptions<DefaultAzureEmailOptions> options,
        ILogger<DefaultAzureEmailProvider> logger,
        IStringLocalizer<DefaultAzureEmailProvider> stringLocalizer)
        : base(options.Value, logger, stringLocalizer)
    {
    }

    public LocalizedString DisplayName => S["Default Azure Communication Service"];
}
