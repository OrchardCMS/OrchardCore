using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public class ContentTypeTransferSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    protected readonly IStringLocalizer S;

    public ContentTypeTransferSettingsDisplayDriver(
        IStringLocalizer<ContentTypeTransferSettingsDisplayDriver> stringLocalizer
        )
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
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

        return Edit(contentTypeDefinition);
    }
}

