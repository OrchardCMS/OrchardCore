using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Media.Fields;

namespace OrchardCore.ContentTransfer.Handlers.Fields;

public sealed class MediaFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    protected readonly IStringLocalizer S;

    public MediaFieldImportHandler(IStringLocalizer<MediaFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context)
    {
        return
        [
            new ImportColumn()
            {
                Name = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_{nameof(MediaField.Paths)}",
                Description = S["A comma-separated list of media paths for {0}", context.ContentPartFieldDefinition.DisplayName()],
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
            var field = context.ContentPart.Get<MediaField>(context.ContentPartFieldDefinition.Name);

            if (field?.Paths?.Length > 0)
            {
                context.Row[knownColumn.Name] = string.Join(",", field.Paths);
            }
        }

        return Task.CompletedTask;
    }
}
