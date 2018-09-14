using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Lucene.Settings
{
    public class ContentPickerFieldLuceneEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
    {
        private readonly LuceneIndexManager _luceneIndexManager;

        public ContentPickerFieldLuceneEditorSettingsDriver(LuceneIndexManager luceneIndexManager)
        {
            _luceneIndexManager = luceneIndexManager;
        }

        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<ContentPickerFieldLuceneEditorSettings>("ContentPickerFieldLuceneEditorSettings_Edit", model =>
            {
                partFieldDefinition.Settings.Populate<ContentPickerFieldLuceneEditorSettings>(model);
                model.Indices = _luceneIndexManager.List().ToArray();
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
            return string.Equals("ContentPickerField", model.FieldDefinition.Name);
        }
    }
}
