using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

public class ShellSettingsEntryManageFeatureDisplayDriver : DisplayDriver<ShellSettingsEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ShellSettingsEntry model, BuildDisplayContext context)
    {
        if (model.ShellSettings is not null &&
            !model.ShellSettings.IsDefaultShell() &&
            context.GroupId.Equals("ActionButtons", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(View("ManageFeaturesActionButtons", model).Location("Content:5").OnGroup(context.GroupId));
        }

        return Task.FromResult<IDisplayResult>(null);
    }
}
