using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors;

/// <summary>
/// Edits how the value of any field is displayed: with or without the name of the field, and when it has no value.
/// </summary>
internal sealed class ContentPartFieldDisplaySettingsDisplayDriver : ContentPartFieldDefinitionDisplayDriver
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<ContentPartFieldDisplaySettingsViewModel>("ContentPartFieldDisplaySettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<ContentPartFieldSettings>();

            model.DisplayFieldName = settings.DisplayFieldName;
            model.AlwaysDisplay = settings.AlwaysDisplay;
        }).Location("Content:after");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new ContentPartFieldDisplaySettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder
            .WithDisplayFieldName(model.DisplayFieldName)
            .WithAlwaysDisplay(model.AlwaysDisplay);

        return Edit(partFieldDefinition, context);
    }
}
