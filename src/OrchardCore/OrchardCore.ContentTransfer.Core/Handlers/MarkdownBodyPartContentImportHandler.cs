using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Markdown.Models;

namespace OrchardCore.ContentTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="MarkdownBodyPart"/> content part.
/// Maps the <c>Markdown</c> property to a single spreadsheet column.
/// </summary>
public sealed class MarkdownBodyPartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public MarkdownBodyPartContentImportHandler(IStringLocalizer<MarkdownBodyPartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(MarkdownBodyPart.Markdown)}",
            Description = S["The markdown body content for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
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

                var markdown = context.Row[column]?.ToString();

                if (markdown == null)
                {
                    continue;
                }

                context.ContentItem.Alter<MarkdownBodyPart>(part =>
                {
                    part.Markdown = markdown;
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
            var part = context.ContentItem.As<MarkdownBodyPart>();

            if (part != null)
            {
                context.Row[_column.Name] = part.Markdown ?? string.Empty;
            }
        }

        return Task.CompletedTask;
    }
}
