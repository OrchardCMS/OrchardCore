using Microsoft.Extensions.Localization;
using OrchardCore.Deployment.Remote.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Remote.Drivers;

/// <summary>
/// Builds the editor of a remote instance, on the pages creating and editing a remote instance, whose fields are
/// grouped in cards by their placement.
/// </summary>
internal sealed class EditRemoteInstanceDisplayDriver : DisplayDriver<EditRemoteInstanceViewModel>
{
    internal readonly IStringLocalizer S;

    public EditRemoteInstanceDisplayDriver(IStringLocalizer<EditRemoteInstanceDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(EditRemoteInstanceViewModel model, BuildEditorContext context)
    {
        return Combine(
            Initialize<EditRemoteInstanceViewModel>("RemoteInstanceGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<EditRemoteInstanceViewModel>("RemoteInstanceCredentials_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(S["Credentials"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(EditRemoteInstanceViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name, m => m.Url, m => m.ClientName, m => m.ApiKey);

        return Edit(model, context);
    }

    // The fields keep the names the pages post, which the controller validates.
    protected override void BuildPrefix(EditRemoteInstanceViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(EditRemoteInstanceViewModel source, EditRemoteInstanceViewModel model)
    {
        model.Id = source.Id;
        model.Name = source.Name;
        model.Url = source.Url;
        model.ClientName = source.ClientName;
        model.ApiKey = source.ApiKey;
    }
}
