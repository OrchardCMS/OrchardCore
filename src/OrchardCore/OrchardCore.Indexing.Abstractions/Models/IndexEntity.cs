using System.Text.Json.Nodes;
using OrchardCore.Entities;

namespace OrchardCore.Indexing.Models;

public sealed class IndexEntity : Entity
{
    public string Id { get; set; }

    public string ProviderName { get; set; }

    public string Type { get; set; }

    public string DisplayText { get; set; }

    public string IndexName { get; set; }

    public string IndexFullName { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string Author { get; set; }

    public string OwnerId { get; set; }

    public IndexEntity Clone()
    {
        return new IndexEntity
        {
            Id = Id,
            DisplayText = DisplayText,
            ProviderName = ProviderName,
            IndexName = IndexName,
            IndexFullName = IndexFullName,
            Type = Type,
            CreatedUtc = CreatedUtc,
            Author = Author,
            OwnerId = OwnerId,
            Properties = Properties.Clone(),
        };
    }
}
