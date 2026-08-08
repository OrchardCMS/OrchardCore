using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTransfer.Handlers.Fields;

public sealed class LocalizationSetContentPickerFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    internal readonly IStringLocalizer S;

    public LocalizationSetContentPickerFieldImportHandler(IStringLocalizer<LocalizationSetContentPickerFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context)
    {
        return
        [
            new ImportColumn()
            {
                Name = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_{nameof(LocalizationSetContentPickerField.LocalizationSets)}",
                Description = S["A comma-separated list of localization set ids for {0}", context.ContentPartFieldDefinition.DisplayName()],
                Type = ImportColumnType.ExportOnly,
            },
        ];
    }

    public Task ImportAsync(ContentFieldImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentFieldExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentPart);
        ArgumentNullException.ThrowIfNull(context.Row);

        var knownColumn = GetColumns(context).FirstOrDefault();

        if (knownColumn != null)
        {
            var field = context.ContentPart.Get<LocalizationSetContentPickerField>(context.ContentPartFieldDefinition.Name);

            if (field?.LocalizationSets?.Length > 0)
            {
                context.Row[knownColumn.Name] = string.Join(",", field.LocalizationSets);
            }
        }

        return Task.CompletedTask;
    }
}
