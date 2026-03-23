using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="ArchiveLaterPart"/> content part.
/// Maps the <c>ScheduledArchiveUtc</c> property to a single spreadsheet column.
/// </summary>
public sealed class ArchiveLaterPartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public ArchiveLaterPartContentImportHandler(IStringLocalizer<ArchiveLaterPartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(ArchiveLaterPart.ScheduledArchiveUtc)}",
            Description = S["The scheduled archive date and time for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
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

                var value = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(value) || !DateTime.TryParse(value.Trim(), out var scheduledArchiveUtc))
                {
                    continue;
                }

                context.ContentItem.Alter<ArchiveLaterPart>(part =>
                {
                    part.ScheduledArchiveUtc = scheduledArchiveUtc;
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
            var part = context.ContentItem.As<ArchiveLaterPart>();

            if (part != null)
            {
                context.Row[_column.Name] = part.ScheduledArchiveUtc;
            }
        }

        return Task.CompletedTask;
    }
}
