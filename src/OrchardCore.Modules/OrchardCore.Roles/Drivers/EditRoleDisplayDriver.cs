using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Roles.ViewModels;

namespace OrchardCore.Roles.Drivers;

/// <summary>
/// Builds the editor of an existing role, whose fields and permissions are grouped in cards by their placement.
/// </summary>
internal sealed class EditRoleDisplayDriver : DisplayDriver<EditRoleViewModel>
{
    internal readonly IStringLocalizer S;

    public EditRoleDisplayDriver(IStringLocalizer<EditRoleDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditRoleViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<EditRoleViewModel>("EditRoleGeneral_Edit", m =>
            {
                m.Role = model.Role;
                m.RoleDescription = model.RoleDescription;
            }).Location(l => l.Zone("Content", "1").Card(S["General"], "10")),

            // The permissions are posted as checkboxes that the controller reads from the form.
            Initialize<RolePermissionsViewModel>("RolePermissions_Edit", m => RolePermissionsEditor.Populate(model.Permissions, m))
                .Location(l => l.Zone("Content", "2").Card(S["Permissions"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditRoleViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.RoleDescription);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller reads.
    protected override void BuildPrefix(EditRoleViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
