using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Drivers
{
    public class ContentPickerFieldElasticEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;

        public ContentPickerFieldElasticEditorSettingsDriver(ElasticIndexSettingsService elasticIndexSettingsService)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
        }

        public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            var model = partFieldDefinition.Settings.ToObject<ContentPickerFieldElasticEditorSettings>();
            model.Indices = (await _elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

            return Copy("ContentPickerFieldElasticEditorSettings_Edit", model)
                .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Elasticsearch")
            {
                var model = new ContentPickerFieldElasticEditorSettings();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                context.Builder.WithSettings(model);
            }

            return await EditAsync(partFieldDefinition, context);
        }

        public override bool CanHandleModel(ContentPartFieldDefinition model)
        {
            return string.Equals("ContentPickerField", model.FieldDefinition.Name);
        }
    }
}
