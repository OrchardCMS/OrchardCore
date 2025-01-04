using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Settings;

namespace OrchardCore.Spatial.Drivers;

public sealed class GeoPointFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<GeoPointField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<GeoPointFieldSettings>("GeoPointFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<GeoPointFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new GeoPointFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return Edit(partFieldDefinition, context);
    }
}
