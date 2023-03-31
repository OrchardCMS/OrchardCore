using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

public class ShellSettingsEntryManageFeatureDisplayDriver : DisplayDriver<ShellSettingsEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ShellSettingsEntry model, BuildDisplayContext context)
    {
        if (model.ShellSettings != null && !model.ShellSettings.IsDefaultShell() && context.GroupId.Equals("ActionButtons", System.StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(View("ManageFeaturesActionButtons", model).Location("Content:5").OnGroup(context.GroupId));
        }

        return Task.FromResult<IDisplayResult>(null);
    }
}
