using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class YoutubeFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<YoutubeField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<YoutubeFieldSettings>("YoutubeFieldSetting_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<YoutubeFieldSettings>();

            model.Hint = settings.Hint;
            model.Label = settings.Label;
            model.Height = settings.Height != default ? settings.Height : 315;
            model.Width = settings.Width != default ? settings.Width : 560;
            model.Required = settings.Required;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new YoutubeFieldSettings();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
