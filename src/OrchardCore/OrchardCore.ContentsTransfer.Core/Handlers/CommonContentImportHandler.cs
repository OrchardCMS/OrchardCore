using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer.Handlers;

public class CommonContentImportHandler : ContentImportHandlerBase, IContentImportHandler
{
    private readonly IContentItemIdGenerator _contentItemIdGenerator;
    protected readonly IStringLocalizer S;

    public CommonContentImportHandler(
        IStringLocalizer<TitlePartContentImportHandler> stringLocalizer,
        IContentItemIdGenerator contentItemIdGenerator)
    {
        _contentItemIdGenerator = contentItemIdGenerator;
        S = stringLocalizer; ;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentContext context)
    {
        return new[]
        {
            new ImportColumn()
            {
                Name = nameof(ContentItem.ContentItemId),
                Description = S["The id for the {0}", context.ContentTypeDefinition.DisplayName],
            },
            new ImportColumn()
            {
                Name = nameof(ContentItem.CreatedUtc),
                Description = S["The UTC created datetime value {0}", context.ContentTypeDefinition.DisplayName],
                Type = ImportColumnType.ExportOnly,
            },
            new ImportColumn()
            {
                Name = nameof(ContentItem.ModifiedUtc),
                Description = S["The UTC last modified datetime value {0}", context.ContentTypeDefinition.DisplayName],
                Type = ImportColumnType.ExportOnly,
            },
        };
    }

    public Task ImportAsync(ContentImportContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

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

        foreach (DataColumn column in context.Columns)
        {
            if (Is(column.ColumnName, nameof(ContentItem.ContentItemId)))
            {
                var contentItemId = context.Row[column]?.ToString();

                if (string.IsNullOrWhiteSpace(contentItemId))
                {
                    continue;
                }

                var fakeId = _contentItemIdGenerator.GenerateUniqueId(new ContentItem());

                if (fakeId.Length == contentItemId.Length)
                {
                    // Just check if the given id matched the fakeId length.
                    context.ContentItem.ContentItemId = contentItemId;
                }

                continue;
            }
        }

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentExportContext context)
    {
        if (context.ContentItem == null)
        {
            throw new ArgumentNullException(nameof(context.ContentItem));
        }

        if (context.Row == null)
        {
            throw new ArgumentNullException(nameof(context.Row));
        }

        // TODO, add settings to allow exporting the following columns.
        context.Row[nameof(ContentItem.ContentItemId)] = context.ContentItem.ContentItemId;
        context.Row[nameof(ContentItem.CreatedUtc)] = context.ContentItem.CreatedUtc;
        context.Row[nameof(ContentItem.ModifiedUtc)] = context.ContentItem.ModifiedUtc;

        return Task.CompletedTask;
    }
}
