using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Remote.Drivers;

/// <summary>
/// Builds the editor of a remote client, on the pages creating and editing a remote client, whose fields are grouped
/// in cards by their placement.
/// </summary>
internal sealed class EditRemoteClientDisplayDriver : DisplayDriver<EditRemoteClientViewModel>
{
    internal readonly IStringLocalizer S;

    public EditRemoteClientDisplayDriver(IStringLocalizer<EditRemoteClientDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditRemoteClientViewModel model, BuildEditorContext context)
    {
        return Initialize<EditRemoteClientViewModel>("RemoteClientCredentials_Edit", m =>
        {
            m.Id = model.Id;
            m.ClientName = model.ClientName;
            m.ApiKey = model.ApiKey;
        }).Location(l => l.Zone("Content", "1").Card(S["Credentials"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditRemoteClientViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ClientName, m => m.ApiKey);

        return Edit(model, context);
    }

    // The fields keep the names the pages post, which the controller validates.
    protected override void BuildPrefix(EditRemoteClientViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
