using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentsTransfer.Handlers;

public abstract class ContentFieldImportHandlerBase : IContentFieldImportHandler
{
    protected readonly IStringLocalizer S;

    public ContentFieldImportHandlerBase(
        IStringLocalizer<ContentFieldImportHandlerBase> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected static bool Is(string columnName, params string[] terms)
    {
        foreach (var term in terms)
        {
            if (columnName.Equals(term, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    protected static bool Is(string columnName, ImportColumn importColumn)
    {
        if (columnName.Equals(importColumn.Name, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var term in importColumn.AdditionalNames ?? Array.Empty<string>())
        {
            if (columnName.Equals(term, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    protected static string[] SplitCellValues(DataRow row, DataColumn column, string seperator = ",")
    {
        return (row[column]?.ToString()?.Split(seperator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) ?? Array.Empty<string>();
    }

    public abstract IReadOnlyCollection<ImportColumn> Columns(ImportContentFieldContext context);

    public abstract Task MapAsync(ContentFieldImportMapContext content);

    public abstract Task MapOutAsync(ContentFieldExportMapContext content);

    public Task ValidateColumnsAsync(ValidateFieldImportContext context)
    {
        var foundColumns = new List<ImportColumn>();
        var knownColumns = Columns(context);

        foreach (DataColumn column in context.Columns)
        {
            foreach (var knownColumn in knownColumns)
            {
                if (Is(column.ColumnName, knownColumn))
                {
                    foundColumns.Add(knownColumn);
                }
            }
        }

        foreach (var knownColumn in knownColumns)
        {
            if (!knownColumn.IsRequired || foundColumns.Any(x => x.Name == knownColumn.Name))
            {
                continue;
            }

            context.ContentValidateResult.Fail(new ValidationResult(S["The column '{0}' is required", knownColumn.Name]));
        }

        return Task.CompletedTask;
    }
}
