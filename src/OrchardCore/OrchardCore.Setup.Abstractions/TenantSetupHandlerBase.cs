using System.Threading.Tasks;
using OrchardCore.Setup.Events;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup;

public class TenantSetupHandlerBase : ITenantSetupHandler
{
    public virtual Task FailedAsync(SetupContext context)
        => Task.CompletedTask;

    public virtual Task SetupAsync(SetupContext context)
        => Task.CompletedTask;

    public virtual Task SucceededAsync()
        => Task.CompletedTask;
}
