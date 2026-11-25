using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers;

/// <summary>
/// Builds the editor of the move of secrets to another store, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class SecretStoreDisplayDriver : DisplayDriver<SecretStoreViewModel>
{
    internal readonly IStringLocalizer S;

    public SecretStoreDisplayDriver(IStringLocalizer<SecretStoreDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(SecretStoreViewModel model, BuildEditorContext context)
    {
        return Initialize<SecretStoreViewModel>("SecretStoreDestination_Edit", m =>
        {
            m.SourceStore = model.SourceStore;
            m.DestinationStore = model.DestinationStore;
            m.Name = model.Name;
            m.Confirm = model.Confirm;
            m.ActiveCount = model.ActiveCount;
            m.AvailableStores = model.AvailableStores;
            m.Results = model.Results;
        }).Location(l => l.Zone("Content", "1").Card(S["Destination"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretStoreViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DestinationStore, m => m.Confirm);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(SecretStoreViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
