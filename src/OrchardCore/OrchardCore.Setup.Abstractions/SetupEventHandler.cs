using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Setup.Events;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup;

public class SetupEventHandler : ISetupEventHandler
{
    public virtual Task Setup(IDictionary<string, object> properties, Action<string, string> reportError)
        => Task.CompletedTask;

    public virtual Task SetupAsync(SetupContext context)
        => Task.CompletedTask;

    public virtual Task FailedAsync(SetupContext context)
        => Task.CompletedTask;

    public virtual Task SucceededAsync()
        => Task.CompletedTask;
}
