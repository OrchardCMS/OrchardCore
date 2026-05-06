using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid.Models;

namespace OrchardCore.ContentTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="LiquidPart"/> content part.
/// Maps the <c>Liquid</c> property to a single spreadsheet column.
/// </summary>
public sealed class LiquidPartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public LiquidPartContentImportHandler(IStringLocalizer<LiquidPartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(LiquidPart.Liquid)}",
            Description = S["The liquid template content for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
        };

        return [_column];
    }

    public Task ImportAsync(ContentPartImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem);
        ArgumentNullException.ThrowIfNull(context.Columns);
        ArgumentNullException.ThrowIfNull(context.Row);

        var knownColumn = GetColumns(context).FirstOrDefault();

        if (knownColumn != null)
        {
            foreach (DataColumn column in context.Columns)
            {
                if (!Is(column.ColumnName, knownColumn))
                {
                    continue;
                }

                var liquid = context.Row[column]?.ToString();

                if (liquid == null)
                {
                    continue;
                }

                context.ContentItem.Alter<LiquidPart>(part =>
                {
                    part.Liquid = liquid;
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentPartExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem);
        ArgumentNullException.ThrowIfNull(context.Row);

        if (_column?.Name != null && context.ContentItem.TryGet<LiquidPart>(out var part))
        {
            context.Row[_column.Name] = part.Liquid ?? string.Empty;
        }

        return Task.CompletedTask;
    }
}
