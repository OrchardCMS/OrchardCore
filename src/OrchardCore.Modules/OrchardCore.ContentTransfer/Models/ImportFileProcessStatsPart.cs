using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentTransfer.Models;

public sealed class ImportFileProcessStatsPart : ContentPart
{
    /// <summary>
    /// Current row being processed.
    /// </summary>
    public int CurrentRow { get; set; }

    /// <summary>
    /// Total processes records.
    /// </summary>

    public int TotalProcessed { get; set; }

    /// <summary>
    /// Total successfully imported records.
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// The index of each row that failed validation.
    /// </summary>
    public HashSet<int> Errors { get; set; }

    /// <summary>
    /// The error message for each failed row.
    /// </summary>
    public Dictionary<int, string> ErrorMessages { get; set; }

    /// <summary>
    /// Total records available if the file.
    /// </summary>
    public int TotalRecords { get; set; }
}
