using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Elasticsearch.Settings
{
    public class ContentPickerFieldElasticsearchEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
    {
        private readonly ElasticsearchIndexSettingsService _elasticIndexSettingsService;

        public ContentPickerFieldElasticsearchEditorSettingsDriver(ElasticsearchIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<ContentPickerFieldElasticsearchEditorSettings>("ContentPickerFieldElasticEditorSettings_Edit", async model =>
            {
                partFieldDefinition.PopulateSettings<ContentPickerFieldElasticsearchEditorSettings>(model);
                model.Indices = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Elasticsearch")
            {
                var model = new ContentPickerFieldElasticsearchEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model);
            }

            return Edit(partFieldDefinition);
        }

        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            return String.Equals("ContentPickerField", model.FieldDefinition.Name);
        }
    }
}
