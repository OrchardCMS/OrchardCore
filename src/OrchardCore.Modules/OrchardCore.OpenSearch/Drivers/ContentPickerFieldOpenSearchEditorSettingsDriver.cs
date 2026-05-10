using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Indexing;
using OrchardCore.OpenSearch.Core.Models;

namespace OrchardCore.OpenSearch.Drivers;

public sealed class ContentPickerFieldOpenSearchEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver
{
    private readonly IIndexProfileStore _store;

    public ContentPickerFieldOpenSearchEditorSettingsDriver(IIndexProfileStore store)
    {
        _store = store;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<ContentPickerFieldOpenSearchEditorSettings>("ContentPickerFieldOpenSearchEditorSettings_Edit", async model =>
        {
            var settings = partFieldDefinition.GetSettings<ContentPickerFieldOpenSearchEditorSettings>();

            model.Index = settings.Index;

            model.Indices = (await _store.GetByProviderAsync(OpenSearchConstants.ProviderName)).Select(x => x.IndexName).ToArray();
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "OpenSearch")
        {
            var model = new ContentPickerFieldOpenSearchEditorSettings();

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
