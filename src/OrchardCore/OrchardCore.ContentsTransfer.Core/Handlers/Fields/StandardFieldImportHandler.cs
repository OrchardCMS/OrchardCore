using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public abstract class StandardFieldImportHandler : ContentFieldImportHandlerBase
{
    public StandardFieldImportHandler(IStringLocalizer<StandardFieldImportHandler> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    public override IReadOnlyCollection<ImportColumn> Columns(ImportContentFieldContext context)
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

    public override async Task MapAsync(ContentFieldImportMapContext context)
    {
        if (context.ContentItem == null)
        {
            throw new ArgumentNullException(nameof(context.ContentItem));
        }

        if (context.Columns == null)
        {
            throw new ArgumentNullException(nameof(context.Columns));
        }

        if (context.Row == null)
        {
            throw new ArgumentNullException(nameof(context.Row));
        }

        var knownColumns = Columns(context);

        foreach (DataColumn column in context.Columns)
        {
            var firstColumn = knownColumns.FirstOrDefault(x => Is(column.ColumnName, x));

            if (firstColumn == null)
            {
                continue;
            }

            var text = context.Row[column]?.ToString();

            await SetValueAsync(context, text);
        }
    }

    public override async Task MapOutAsync(ContentFieldExportMapContext context)
    {
        if (context.ContentItem == null)
        {
            throw new ArgumentNullException(nameof(context.ContentItem));
        }

        if (context.Row == null)
        {
            throw new ArgumentNullException(nameof(context.Row));
        }

        var firstColumn = Columns(context).FirstOrDefault();

        if (firstColumn != null)
        {
            context.Row[firstColumn.Name] = await GetValueAsync(context);
        }
    }

    protected abstract Task<object> GetValueAsync(ContentFieldExportMapContext context);

    protected abstract Task SetValueAsync(ContentFieldImportMapContext context, string value);

    protected virtual string Description(ImportContentFieldContext context) => string.Empty;

    protected virtual bool IsRequired(ImportContentFieldContext context) => false;

    protected virtual string[] GetValidValues(ImportContentFieldContext context) => Array.Empty<string>();

    protected abstract string BindingPropertyName { get; }
}
