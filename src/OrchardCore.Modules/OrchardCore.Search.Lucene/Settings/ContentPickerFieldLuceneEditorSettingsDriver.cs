using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Lucene.Settings;

public sealed class ContentPickerFieldLuceneEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IIndexEntityStore _indexEntityStore;

    public ContentPickerFieldLuceneEditorSettingsDriver(IIndexEntityStore indexEntityStore)
    {
        _indexEntityStore = indexEntityStore;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<ContentPickerFieldLuceneEditorSettings>("ContentPickerFieldLuceneEditorSettings_Edit", async model =>
        {
            var settings = partFieldDefinition.GetSettings<ContentPickerFieldLuceneEditorSettings>();

            model.Index = settings.Index;
            model.Indices = (await _indexEntityStore.GetByProviderAsync(LuceneConstants.ProviderName)).Select(x => x.IndexName).ToArray();
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

        return Edit(partFieldDefinition, context);
    }

    public override bool CanHandleModel(ContentPartFieldDefinition model)
    {
        return string.Equals("ContentPickerField", model.FieldDefinition.Name, StringComparison.Ordinal);
    }
}
