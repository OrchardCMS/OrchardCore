using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentsTransfer.Models;

public class ImportFileProcessStatsPart : ContentPart
{
    public int CurrentRow { get; set; }


    public int TotalProcessed { get; set; }

    /// <summary>
    /// The index of each row that failed validation.
    /// </summary>
    public HashSet<int> Errors { get; set; }
}
