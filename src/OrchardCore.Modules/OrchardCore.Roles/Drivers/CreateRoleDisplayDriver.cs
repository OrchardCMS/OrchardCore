using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Roles.ViewModels;

namespace OrchardCore.Roles.Drivers;

/// <summary>
/// Builds the editor of a new role, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class CreateRoleDisplayDriver : DisplayDriver<CreateRoleViewModel>
{
    internal readonly IStringLocalizer S;

    public CreateRoleDisplayDriver(IStringLocalizer<CreateRoleDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(CreateRoleViewModel model, BuildEditorContext context)
    {
        return Initialize<CreateRoleViewModel>("CreateRoleGeneral_Edit", m =>
        {
            m.RoleName = model.RoleName;
            m.RoleDescription = model.RoleDescription;
        }).Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(CreateRoleViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.RoleName, m => m.RoleDescription);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(CreateRoleViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
