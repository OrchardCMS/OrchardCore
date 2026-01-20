using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Drivers;

public sealed class ContentPickerFieldElasticEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IIndexProfileStore _store;

    public ContentPickerFieldElasticEditorSettingsDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<ContentPickerFieldElasticEditorSettings>("ContentPickerFieldElasticEditorSettings_Edit", async model =>
        {
            var settings = partFieldDefinition.GetSettings<ContentPickerFieldElasticEditorSettings>();

            model.Index = settings.Index;

            model.Indices = (await _store.GetByProviderAsync(ElasticsearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "Elasticsearch")
        {
            var model = new ContentPickerFieldElasticEditorSettings();

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
