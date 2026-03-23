namespace OrchardCore.ContentsTransfer;

public sealed class ContentTransferEntryQueryResult
{
    public IEnumerable<ContentTransferEntry> Entries { get; set; }

    public int TotalCount { get; set; }
}
