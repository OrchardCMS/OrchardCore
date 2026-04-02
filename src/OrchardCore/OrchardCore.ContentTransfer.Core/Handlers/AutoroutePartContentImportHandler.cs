using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="AutoroutePart"/> content part.
/// Maps the <c>Path</c> property to a single spreadsheet column.
/// </summary>
public sealed class AutoroutePartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    internal readonly IStringLocalizer S;

    private ImportColumn _column;

    public AutoroutePartContentImportHandler(IStringLocalizer<AutoroutePartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(AutoroutePart.Path)}",
            Description = S["The URL path for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
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

                var path = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                context.ContentItem.Alter<AutoroutePart>(part =>
                {
                    part.Path = path;
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

        if (_column?.Name != null && context.ContentItem.TryGet<AutoroutePart>(out var part))
        {
            context.Row[_column.Name] = part.Path ?? string.Empty;
        }

        return Task.CompletedTask;
    }
}
