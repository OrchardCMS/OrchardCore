using System;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretActivator
{
    public virtual Type Type => typeof(SecretBase);

    public virtual SecretBase Create() => new();
}
