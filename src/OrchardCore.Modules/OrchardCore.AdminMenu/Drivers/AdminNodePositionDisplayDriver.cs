using Microsoft.Extensions.Localization;
using OrchardCore.AdminMenu.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Drivers;

/// <summary>
/// Adds the selection priority and the position of an existing admin node to its editor, in the "Position" card.
/// </summary>
internal sealed class AdminNodePositionDisplayDriver : DisplayDriver<MenuItem>
{
    internal readonly IStringLocalizer S;

    public AdminNodePositionDisplayDriver(IStringLocalizer<AdminNodePositionDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(MenuItem treeNode, BuildEditorContext context)
    {
        // The page creating a node doesn't edit its position.
        if (context.IsNew)
        {
            return null;
        }

        return Initialize<AdminNodeEditViewModel>("AdminNodePosition_Edit", m =>
        {
            m.Priority = treeNode.Priority;
            m.Position = treeNode.Position;
        }).Location(l => l.Zone("Content", "10").Card(S["Position"], "20"));
    }

    public override async Task<IDisplayResult> UpdateAsync(MenuItem treeNode, UpdateEditorContext context)
    {
        if (context.IsNew)
        {
            return null;
        }

        var model = new AdminNodeEditViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Priority, m => m.Position);

        treeNode.Priority = model.Priority;
        treeNode.Position = model.Position;

        return Edit(treeNode, context);
    }

    // The fields keep the names the page posts.
    protected override void BuildPrefix(MenuItem model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
