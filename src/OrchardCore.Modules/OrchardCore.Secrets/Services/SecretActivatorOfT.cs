using System;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretActivator<TSecret> : SecretActivator where TSecret : Secret, new()
{
    public override Type Type => typeof(TSecret);

    public override Secret Create() => new TSecret();
}
