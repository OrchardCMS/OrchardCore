using System;
using OrchardCore.ContentTransfer;
using YesSql.Indexes;

namespace OrchardCore.ContentsTransfer.Indexes;

public class ContentTransferEntryIndex : MapIndex
{
    /// <summary>
    /// The logical identifier of the entry.
    /// </summary>
    public string EntryId { get; set; }

    public ContentTransferEntryStatus Status { get; set; }

    /// <summary>
    /// When the content item has been created or first published.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// The content type being imported.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The user id of the user who created this entry.
    /// </summary>
    public string Owner { get; set; }
}

public class ContentTransferEntryIndexProvider : IndexProvider<ContentTransferEntry>
{
    public override void Describe(DescribeContext<ContentTransferEntry> context)
    {
        context.For<ContentTransferEntryIndex>()
            .Map(entry => new ContentTransferEntryIndex()
            {
                EntryId = entry.EntryId,
                ContentType = entry.ContentType,
                CreatedUtc = entry.CreatedUtc,
                Owner = entry.Owner,
                Status = entry.Status,
            });
    }
}
