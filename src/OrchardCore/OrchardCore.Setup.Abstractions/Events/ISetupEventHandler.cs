using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Events;

public interface ISetupEventHandler
{
    [Obsolete($"This method is obsolete and will be removed in future releases. Please use '{nameof(SetupAsync)}' instead.", false)]
    Task Setup(IDictionary<string, object> properties, Action<string, string> reportError) => Task.CompletedTask;

    /// <summary>
    /// Called during the process of setting up a new tenant.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
#pragma warning disable CS0618 // Type or member is obsolete
    Task SetupAsync(SetupContext context) => Setup(context.Properties, (key, message) => context.Errors[key] = message);
#pragma warning restore CS0618 // Type or member is obsolete

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
