using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentsTransfer.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentsTransfer.Drivers;

public class ImportableContentTypeSettingsDisplayDriver : ContentTypeDefinitionDisplayDriver
{
    protected readonly IStringLocalizer S;

    public ImportableContentTypeSettingsDisplayDriver(
        IStringLocalizer<ImportableContentTypeSettingsDisplayDriver> stringLocalizer
        )
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentTypeDefinition contentTypeDefinition)
    {
        return Initialize<ImportableContentTypeSettings>("ImportableContentTypeSettings_Edit", model =>
        {
            var settings = contentTypeDefinition.GetSettings<ImportableContentTypeSettings>();
            model.AllowBulkImport = settings.AllowBulkImport;
            model.AllowBulkExport = settings.AllowBulkExport;

        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypeDefinition contentTypeDefinition, UpdateTypeEditorContext context)
    {
        var model = new ImportableContentTypeSettings();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            context.Builder.WithSettings(model);
        }

        return Edit(contentTypeDefinition);
    }
}

