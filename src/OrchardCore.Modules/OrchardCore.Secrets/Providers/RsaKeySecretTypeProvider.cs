using Microsoft.Extensions.Localization;

namespace OrchardCore.Secrets.Providers;

public sealed class RsaKeySecretTypeProvider : SecretTypeProvider<RsaKeySecret>
{
    private readonly IStringLocalizer S;

    public RsaKeySecretTypeProvider(IStringLocalizer<RsaKeySecretTypeProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override string DisplayName => S["RSA Key"];

    public override string Description => S["Stores RSA cryptographic keys for signing, encryption, or verification."];
}
