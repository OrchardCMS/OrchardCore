using System;

namespace OrchardCore.Secrets;

public class SecretFactory<TSecret> : ISecretFactory where TSecret : Secret, new()
{
    public Type Type => typeof(TSecret);

    public Secret Create() => new TSecret();
}
