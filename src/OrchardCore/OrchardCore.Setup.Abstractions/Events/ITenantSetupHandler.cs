using System.Threading.Tasks;
using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Events;

public interface ITenantSetupHandler
{
    Task SettingUpAsync(SetupContext context);

    Task CompletedAsync(CompletedSetupContext context);
}
