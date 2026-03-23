using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Html.Models;

namespace OrchardCore.ContentsTransfer.Handlers;

/// <summary>
/// Handles import and export of the <see cref="HtmlBodyPart"/> content part.
/// Maps the <c>Html</c> property to a single spreadsheet column.
/// </summary>
public sealed class HtmlBodyPartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    private ImportColumn _column;

    internal readonly IStringLocalizer S;

    public HtmlBodyPartContentImportHandler(IStringLocalizer<HtmlBodyPartContentImportHandler> localizer)
    {
        S = localizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        _column ??= new ImportColumn()
        {
            Name = $"{context.ContentTypePartDefinition.Name}_{nameof(HtmlBodyPart.Html)}",
            Description = S["The HTML body content for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
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

                var html = context.Row[column]?.ToString();

                if (html == null)
                {
                    continue;
                }

                context.ContentItem.Alter<HtmlBodyPart>(part =>
                {
                    part.Html = html;
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
            var part = context.ContentItem.As<HtmlBodyPart>();

            if (part != null)
            {
                context.Row[_column.Name] = part.Html ?? string.Empty;
            }
        }

        return Task.CompletedTask;
    }
}
