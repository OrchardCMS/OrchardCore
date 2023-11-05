using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer.Models;

public class ImportFileProcessStatsPart : ContentPart
{
    public int CurrentRow { get; set; }

    public int TotalErrors { get; set; }

    public int TotalSuccesses { get; set; }
}
