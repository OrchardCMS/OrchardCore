using OrchardCore.Entities;

namespace OrchardCore.Indexing.Models;

public sealed class IndexProfile : Entity
{
    public string Id { get; set; }

    public string ProviderName { get; set; }

    public string Type { get; set; }

    /// <summary>
    /// A unique name for the index.
    /// </summary>
    public string Name { get; set; }

    public string IndexName { get; set; }

    /// <summary>
    /// This is the index name that is used by the provider which may contains
    /// prefixes like tenant name or predefined constant.
    /// </summary>
    public string IndexFullName { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string Author { get; set; }

    public string OwnerId { get; set; }
}
