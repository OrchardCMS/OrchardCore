using System.Threading.Tasks;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Events;

public interface ITenantSetupHandler
{
    /// <summary>
    /// Called during the process of setting up a new tenant.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SetupAsync(SetupContext context);

    /// <summary>
    /// Called when a new tenant is successfully setup.
    /// </summary>
    /// <returns></returns>
    Task SucceededAsync();

    /// <summary>
    /// Called when a tenant fails to setup.
    /// </summary>
    /// <returns></returns>
    Task FailedAsync(SetupContext context);
}
