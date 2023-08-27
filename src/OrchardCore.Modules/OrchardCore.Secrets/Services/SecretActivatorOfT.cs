using System;

namespace OrchardCore.Secrets.Services;

public class SecretActivator<TSecret> : SecretActivator where TSecret : Secret, new()
{
    public override Type Type => typeof(TSecret);

    public override Secret Create() => new TSecret();
}

public class SecretActivator
{
    public virtual Type Type => typeof(Secret);

    public virtual Secret Create() => new();
}

public interface ISecretActivator
{
    Type Type { get; }
    Secret Create();
}
