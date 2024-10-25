using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Email.Azure.Models;

namespace OrchardCore.Email.Azure.Services;

public class AzureEmailProvider : AzureEmailProviderBase
{
    public const string TechnicalName = "Azure";

    public AzureEmailProvider(
        IOptions<AzureEmailOptions> options,
        IEmailAddressValidator emailAddressValidator,
        ILogger<AzureEmailProvider> logger,
        IStringLocalizer<AzureEmailProvider> stringLocalizer)
        : base(options.Value, emailAddressValidator, logger, stringLocalizer)
    {
    }

    public override LocalizedString DisplayName
        => S["Azure Communication Services"];
}
