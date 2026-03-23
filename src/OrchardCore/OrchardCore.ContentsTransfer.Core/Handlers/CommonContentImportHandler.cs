using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers;

public sealed class CommonContentImportHandler : ContentImportHandlerBase, IContentImportHandler
{
    private readonly IContentItemIdGenerator _contentItemIdGenerator;

    internal readonly IStringLocalizer S;

    public CommonContentImportHandler(
        IStringLocalizer<TitlePartContentImportHandler> stringLocalizer,
        IContentItemIdGenerator contentItemIdGenerator)
    {
        _contentItemIdGenerator = contentItemIdGenerator;
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentContext context)
    {
        var columns = new List<ImportColumn>
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

        if (context.ContentTypeDefinition.IsVersionable())
        {
            columns.Insert(1, new ImportColumn()
            {
                Name = nameof(ContentItem.ContentItemVersionId),
                Description = S["The version id for the {0}", context.ContentTypeDefinition.DisplayName],
            });
        }

        return columns;
    }

    public Task ImportAsync(ContentImportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

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
            }
            else if (context.ContentTypeDefinition.IsVersionable() && Is(column.ColumnName, nameof(ContentItem.ContentItemVersionId)))
            {
                var contentItemVersionId = context.Row[column]?.ToString();

                if (!string.IsNullOrWhiteSpace(contentItemVersionId))
                {
                    var fakeId = _contentItemIdGenerator.GenerateUniqueId(new ContentItem());

                    if (fakeId.Length == contentItemVersionId.Length)
                    {
                        context.ContentItem.ContentItemVersionId = contentItemVersionId;
                    }
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentExportContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentItem, nameof(context.ContentItem));
        ArgumentNullException.ThrowIfNull(context.Row, nameof(context.Row));

        context.Row[nameof(ContentItem.ContentItemId)] = context.ContentItem.ContentItemId;
        if (context.ContentTypeDefinition.IsVersionable())
        {
            context.Row[nameof(ContentItem.ContentItemVersionId)] = context.ContentItem.ContentItemVersionId;
        }
        context.Row[nameof(ContentItem.CreatedUtc)] = context.ContentItem.CreatedUtc;
        context.Row[nameof(ContentItem.ModifiedUtc)] = context.ContentItem.ModifiedUtc;

        return Task.CompletedTask;
    }
}
