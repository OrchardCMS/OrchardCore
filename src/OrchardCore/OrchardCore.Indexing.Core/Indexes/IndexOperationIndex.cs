using YesSql.Indexes;

namespace OrchardCore.Indexing.Core.Indexes;

/// <summary>Indexes tenant-local operation identifiers.</summary>
public sealed class IndexOperationIndex : MapIndex
{
    /// <summary>Gets or sets the opaque operation identifier.</summary>
    public string OperationId { get; set; }
}
