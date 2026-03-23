using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.ContentsTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="PublishLaterPart"/> content part.
/// Maps the <c>ScheduledPublishUtc</c> property to a single spreadsheet column.
/// </summary>
public sealed class PublishLaterPartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public PublishLaterPartContentImportHandler(IStringLocalizer<PublishLaterPartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(PublishLaterPart.ScheduledPublishUtc)}",
            Description = S["The scheduled publish date and time for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
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

                if (string.IsNullOrWhiteSpace(value) || !DateTime.TryParse(value.Trim(), out var scheduledPublishUtc))
                {
                    continue;
                }

                context.ContentItem.Alter<PublishLaterPart>(part =>
                {
                    part.ScheduledPublishUtc = scheduledPublishUtc;
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
            var part = context.ContentItem.As<PublishLaterPart>();

            if (part != null)
            {
                context.Row[_column.Name] = part.ScheduledPublishUtc;
            }
        }

        return Task.CompletedTask;
    }
}
