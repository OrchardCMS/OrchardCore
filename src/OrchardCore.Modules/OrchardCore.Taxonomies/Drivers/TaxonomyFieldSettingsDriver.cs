using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Settings;

namespace OrchardCore.Taxonomies.Drivers;

public sealed class TaxonomyFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TaxonomyField>
{
    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<TaxonomyFieldSettings>("TaxonomyFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<TaxonomyFieldSettings>();

            model.Hint = settings.Hint;
            model.Required = settings.Required;
            model.TaxonomyContentItemId = settings.TaxonomyContentItemId;
            model.Unique = settings.Unique;
            model.LeavesOnly = settings.LeavesOnly;
            model.Open = settings.Open;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new TaxonomyFieldSettings();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(model);

        return await EditAsync(partFieldDefinition, context);
    }
}
