using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentFields.Settings;

public sealed class TextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TextField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<TextFieldSettings>("TextFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<TextFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
            model.DefaultValue = settings.DefaultValue;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new TextFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
