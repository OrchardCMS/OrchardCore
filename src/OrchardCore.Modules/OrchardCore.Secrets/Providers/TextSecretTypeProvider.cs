using Microsoft.Extensions.Localization;

namespace OrchardCore.Secrets.Providers;

public sealed class TextSecretTypeProvider : SecretTypeProvider<TextSecret>
{
    private readonly IStringLocalizer S;

    public TextSecretTypeProvider(IStringLocalizer<TextSecretTypeProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override string DisplayName => S["Text Secret"];

    public override string Description => S["Stores a text value such as passwords, API keys, or connection strings."];
}
