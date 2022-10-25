using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

public class ShellSettingsEntryDisplayDriver : DisplayDriver<ShellSettingsEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ShellSettingsEntry model, BuildDisplayContext context)
    {
        if (context.GroupId.Equals("ActionButtons", System.StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(View("TenantActionButtons", model).Location("Content:5").OnGroup(context.GroupId));
        }

        if (context.GroupId.Equals("Tags", System.StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(View("TenantTags", model).Location("Content:5").OnGroup(context.GroupId));
        }

        return Task.FromResult<IDisplayResult>(null);
    }
}
