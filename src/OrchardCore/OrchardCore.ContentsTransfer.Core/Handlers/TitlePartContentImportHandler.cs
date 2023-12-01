using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Title.Models;

namespace OrchardCore.ContentsTransfer.Handlers;

public class TitlePartContentImportHandler : ContentImportHandlerBase, IContentPartImportHandler
{
    protected readonly IStringLocalizer S;
    private ImportColumn _column;

    public TitlePartContentImportHandler(IStringLocalizer<TitlePartContentImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentPartContext context)
    {
        if (_column == null)
        {
            var settings = context.ContentTypePartDefinition.GetSettings<TitlePartSettings>();

            if (settings.Options == TitlePartOptions.Editable || settings.Options == TitlePartOptions.EditableRequired)
            {
                _column = new ImportColumn()
                {
                    Name = $"{context.ContentTypePartDefinition.Name}_{nameof(TitlePart.Title)}",
                    Description = S["The title for the {0}", context.ContentTypePartDefinition.ContentTypeDefinition.DisplayName],
                    IsRequired = settings.Options == TitlePartOptions.EditableRequired,
                };
            }
        }

        if (_column == null)
        {
            return Array.Empty<ImportColumn>();
        }

        return new[] { _column };
    }

    public Task ImportAsync(ContentPartImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Columns, nameof(context.Columns));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        if (_column?.Name != null)
        {
            foreach (DataColumn column in context.Columns)
            {
                if (!Is(column.ColumnName, _column))
                {
                    continue;
                }
                var title = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(title))
                {
                    continue;
                }

                context.ContentItem.DisplayText = title;
                context.ContentItem.Alter<TitlePart>(part =>
                {
                    part.Title = title.Trim();
                });
            }
        }

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentPartExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        if (_column?.Name != null)
        {
            context.Row[_column.Name] = context.ContentItem.DisplayText;
        }

        return Task.CompletedTask;
    }
}
