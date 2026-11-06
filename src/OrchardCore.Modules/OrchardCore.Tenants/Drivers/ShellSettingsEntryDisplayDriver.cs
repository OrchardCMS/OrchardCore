using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Drivers;

/// <summary>
/// Builds the row of the tenants admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>Tenants</c> renders.
/// </summary>
public sealed class ShellSettingsEntryDisplayDriver : DisplayDriver<ShellSettingsEntry>
{
    private readonly IShellHost _shellHost;

    public ShellSettingsEntryDisplayDriver(IShellHost shellHost)
    {
        _shellHost = shellHost;
    }

    public override Task<IDisplayResult> DisplayAsync(ShellSettingsEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ShellSettingsEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            View("ShellSettingsEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("ShellSettingsEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("ShellSettingsEntry_Category_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Category:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Category:5"),
            View("ShellSettingsEntry_Database_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Database:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Database:5"),
            View("ShellSettingsEntry_Recipe_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Recipe:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Recipe:5"),
            View("ShellSettingsEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Tags:5"),
            View("ShellSettingsEntry_State_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "State:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "State:5"),
            Initialize<ShellSettingsEntryActivityViewModel>("ShellSettingsEntryActivity_SummaryAdmin", model =>
            {
                _shellHost.TryGetShellContext(entry.Name, out var shellContext);
                model.Update(shellContext, global::System.Environment.MachineName);
            })
                .RenderWhen(() => Task.FromResult(entry.ShellSettings.IsRunning()))
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Activity:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Activity:5"),
            View("ShellSettingsEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
