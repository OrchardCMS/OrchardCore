using Microsoft.Extensions.Localization;

namespace OrchardCore.Secrets.Providers;

public sealed class X509SecretTypeProvider : SecretTypeProvider<X509Secret>
{
    private readonly IStringLocalizer S;

    public X509SecretTypeProvider(IStringLocalizer<X509SecretTypeProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override string DisplayName => S["X.509 Certificate"];

    public override string Description => S["References an X.509 certificate from the operating system's certificate store."];
}
