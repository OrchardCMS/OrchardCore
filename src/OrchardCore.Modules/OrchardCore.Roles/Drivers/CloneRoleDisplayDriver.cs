using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Roles.ViewModels;

namespace OrchardCore.Roles.Drivers;

/// <summary>
/// Builds the editor of the clone of a role, whose fields and permissions are grouped in cards by their placement.
/// </summary>
internal sealed class CloneRoleDisplayDriver : DisplayDriver<CloneRoleViewModel>
{
    internal readonly IStringLocalizer S;

    public CloneRoleDisplayDriver(IStringLocalizer<CloneRoleDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CloneRoleViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<CloneRoleViewModel>("CloneRoleGeneral_Edit", m =>
            {
                m.Role = model.Role;
                m.Name = model.Name;
                m.RoleDescription = model.RoleDescription;
            }).Location(l => l.Zone("Content", "1").Card(S["General"], "10")),

            // The permissions are posted as checkboxes that the controller reads from the form.
            Initialize<RolePermissionsViewModel>("RolePermissions_Edit", m => RolePermissionsEditor.Populate(model.Permissions, m))
                .Location(l => l.Zone("Content", "2").Card(S["Permissions"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(CloneRoleViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.RoleDescription);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller reads.
    protected override void BuildPrefix(CloneRoleViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
