using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Events;

public interface ISetupEventHandler
{
    [Obsolete($"This method is obsolete and will be removed in future releases. Please use '{nameof(SetupAsync)}' instead.", false)]
    Task Setup(IDictionary<string, object> properties, Action<string, string> reportError);

    /// <summary>
    /// Called during the process of setting up a new tenant.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SetupAsync(SetupContext context) => Task.CompletedTask;

    /// <summary>
    /// Called when a tenant fails to setup.
    /// </summary>
    /// <returns></returns>
    Task FailedAsync(SetupContext context) => Task.CompletedTask;

    /// <summary>
    /// Called when a new tenant is successfully setup.
    /// </summary>
    /// <returns></returns>
    Task SucceededAsync() => Task.CompletedTask;
}
