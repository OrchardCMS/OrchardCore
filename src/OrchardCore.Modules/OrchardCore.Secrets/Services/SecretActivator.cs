using System;

namespace OrchardCore.Secrets.Services;

public class SecretActivator
{
    public virtual Type Type => typeof(Secret);

    public virtual Secret Create() => new();
}
