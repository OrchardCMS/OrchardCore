using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public sealed class UserPickerFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    private readonly IStringLocalizer S;

    public UserPickerFieldImportHandler(IStringLocalizer<UserPickerFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context)
    {
        return
        [
            new ImportColumn()
            {
                Name = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_{nameof(UserPickerField.UserIds)}",
                Description = S["A comma-separated list of user ids for {0}", context.ContentPartFieldDefinition.DisplayName()],
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
            var field = context.ContentPart.Get<UserPickerField>(context.ContentPartFieldDefinition.Name);

            if (field?.UserIds?.Length > 0)
            {
                context.Row[knownColumn.Name] = string.Join(",", field.UserIds);
            }
        }

        return Task.CompletedTask;
    }
}
