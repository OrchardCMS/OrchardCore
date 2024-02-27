using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.ContentsTransfer.Handlers;

public abstract class StandardFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context)
    {
        return new[]
        {
            new ImportColumn()
            {
                Name = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_{BindingPropertyName}",
                Description = Description(context),
                IsRequired = IsRequired(context),
                ValidValues = GetValidValues(context),
            }
        };
    }

    public async Task ImportAsync(ContentFieldImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Columns, nameof(context.Columns));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        var knownColumns = GetColumns(context);

        foreach (DataColumn column in context.Columns)
        {
            var knownColumn = knownColumns.FirstOrDefault(x => Is(column.ColumnName, x));

            if (knownColumn == null)
            {
                continue;
            }

            await SetValueAsync(context, context.Row[column]?.ToString());
        }
    }

    public async Task ExportAsync(ContentFieldExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        var knownColumn = GetColumns(context).FirstOrDefault();

        if (knownColumn != null)
        {
            context.Row[knownColumn.Name] = await GetValueAsync(context);
        }
    }

    protected virtual string Description(ImportContentFieldContext context)
        => string.Empty;

    protected virtual bool IsRequired(ImportContentFieldContext context)
        => false;

    protected virtual string[] GetValidValues(ImportContentFieldContext context)
        => [];

    protected abstract Task<object> GetValueAsync(ContentFieldExportMapContext context);

    protected abstract Task SetValueAsync(ContentFieldImportMapContext context, string value);

    protected abstract string BindingPropertyName { get; }
}
