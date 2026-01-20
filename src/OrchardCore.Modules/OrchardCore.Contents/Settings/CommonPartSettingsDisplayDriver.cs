using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Lists.Settings;

public sealed class CommonPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<CommonPart>
{
    public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        return Initialize<CommonPartSettingsViewModel>("CommonPartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<CommonPartSettings>();
            model.DisplayDateEditor = settings.DisplayDateEditor;
            model.DisplayOwnerEditor = settings.DisplayOwnerEditor;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new CommonPartSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(new CommonPartSettings
        {
            DisplayDateEditor = model.DisplayDateEditor,
            DisplayOwnerEditor = model.DisplayOwnerEditor,
        });

        return Edit(contentTypePartDefinition, context);
    }
}
