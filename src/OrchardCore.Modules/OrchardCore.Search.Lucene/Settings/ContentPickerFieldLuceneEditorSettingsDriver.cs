using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Search.Lucene.Settings
{
    public class ContentPickerFieldLuceneEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
    {
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;

        public ContentPickerFieldLuceneEditorSettingsDriver(LuceneIndexSettingsService luceneIndexSettingsService)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
        }

        public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
        {
            var model = partFieldDefinition.Settings.ToObject<ContentPickerFieldLuceneEditorSettings>();
            model.Indices = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();

            return Copy("ContentPickerFieldLuceneEditorSettings_Edit", model)
                .Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Lucene")
            {
                var model = new ContentPickerFieldLuceneEditorSettings();

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
