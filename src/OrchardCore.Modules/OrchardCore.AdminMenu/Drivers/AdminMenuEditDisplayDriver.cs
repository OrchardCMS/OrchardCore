using Microsoft.Extensions.Localization;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminMenu.Drivers;

/// <summary>
/// Builds the editor of an existing admin menu, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class AdminMenuEditDisplayDriver : DisplayDriver<AdminMenuEditViewModel>
{
    internal readonly IStringLocalizer S;

    public AdminMenuEditDisplayDriver(IStringLocalizer<AdminMenuEditDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AdminMenuEditViewModel model, BuildEditorContext context)
    {
        return Initialize<AdminMenuEditViewModel>("AdminMenuEditGeneral_Edit", m =>
        {
            m.Id = model.Id;
            m.Name = model.Name;
        }).Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AdminMenuEditViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(AdminMenuEditViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
