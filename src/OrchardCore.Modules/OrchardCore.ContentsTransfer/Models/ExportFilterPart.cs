using System;

namespace OrchardCore.ContentsTransfer.Models;

/// <summary>
/// Stores export filter criteria on a <see cref="ContentTransferEntry"/> for queued exports.
/// This allows the background task to apply the same filters the user selected.
/// </summary>
public sealed class ExportFilterPart
{
    public bool PublishedOnly { get; set; } = true;

    public bool LatestOnly { get; set; }

    public bool AllVersions { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public DateTime? ModifiedFrom { get; set; }

    public DateTime? ModifiedTo { get; set; }

    public string Owners { get; set; }
}
