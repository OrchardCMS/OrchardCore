using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Settings
{
    public class TaxonomyFieldTagsEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<TaxonomyField>
    {
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<TaxonomyFieldTagsEditorSettings>("TaxonomyFieldTagsEditorSettings_Edit", model =>
            {
                var settings = partFieldDefinition.Settings.ToObject<TaxonomyFieldTagsEditorSettings>();

                model.Open = settings.Open;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Tags")
            {
                var model = new TaxonomyFieldTagsEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model);
            }

            return Edit(partFieldDefinition);
        }
    }
}
