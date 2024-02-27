using System.Collections.Generic;

namespace OrchardCore.ContentsTransfer.Models;

public class ContentTransferEntryQueryResult
{
    public IEnumerable<ContentTransferEntry> Entries { get; set; }

    public int TotalCount { get; set; }
}
