using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public sealed class ContentTypeTransferSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    private readonly IStringLocalizer S;

    public ContentTypeTransferSettingsDisplayDriver(IStringLocalizer<ContentTypeTransferSettingsDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition, BuildEditorContext context)
    {
        return Initialize<ContentTypeTransferSettings>("ContentTypeTransferSettings_Edit", model =>
        {
            var settings = contentTypeDefinition.GetSettings<ContentTypeTransferSettings>();
            model.AllowBulkImport = settings.AllowBulkImport;
            model.AllowBulkExport = settings.AllowBulkExport;
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ContentTypeTransferSettings();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            context.Builder.WithSettings(model);
        }

        return Edit(contentTypeDefinition, context);
    }
}
