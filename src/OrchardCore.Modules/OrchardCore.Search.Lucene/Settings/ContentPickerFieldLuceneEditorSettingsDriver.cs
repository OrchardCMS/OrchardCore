using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
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

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<ContentPickerFieldLuceneEditorSettings>("ContentPickerFieldLuceneEditorSettings_Edit", async model =>
            {
                partFieldDefinition.PopulateSettings<ContentPickerFieldLuceneEditorSettings>(model);
                model.Indices = (await _luceneIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray();
            }).Location("Editor");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            if (partFieldDefinition.Editor() == "Lucene")
            {
                var model = new ContentPickerFieldLuceneEditorSettings();

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
