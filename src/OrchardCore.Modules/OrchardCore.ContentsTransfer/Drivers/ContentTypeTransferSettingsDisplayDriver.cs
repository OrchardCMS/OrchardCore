using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentsTransfer.ViewModels;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public sealed class ContentTypeTransferSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<ContentTypeTransferSettingsViewModels>("ContentTypeTransferSettings_Edit", model =>
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();
            model.AllowBulkImport = settings.AllowBulkImport;
            model.AllowBulkExport = settings.AllowBulkExport;
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ContentTypeTransferSettingsViewModels();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        context.Builder.WithSettings(new ContentTypeTransferSettings
        {
            AllowBulkImport = model.AllowBulkImport,
            AllowBulkExport = model.AllowBulkExport,
        });

        return Edit(contentTypeDefinition, context);
    }
}
