using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="AliasPart"/> content part.
/// Maps the <c>Alias</c> property to a single spreadsheet column.
/// </summary>
public sealed class AliasPartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public AliasPartContentImportHandler(IStringLocalizer<AliasPartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(AliasPart.Alias)}",
            Description = S["The alias for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
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

                var alias = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(alias))
                {
                    continue;
                }

                context.ContentItem.Alter<AliasPart>(part =>
                {
                    part.Alias = alias;
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

        if (_column?.Name != null)
        {
            var part = context.ContentItem.As<AliasPart>();

            if (part != null)
            {
                context.Row[_column.Name] = part.Alias ?? string.Empty;
            }
        }

        return Task.CompletedTask;
    }
}
