using System.Text.Json.Nodes;
using OrchardCore.Catalogs;

namespace OrchardCore.Security.Core;

public class Credential : CatalogItem, INameAwareModel, ISourceAwareModel, IDisplayTextAwareModel, ICloneable<Credential>
{
    public string DisplayText { get; set; }

    public string Name { get; set; }

    public string Source { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string Author { get; set; }

    public string OwnerId { get; set; }

    public Credential Clone()
    {
        return new Credential
        {
            ItemId = ItemId,
            Name = Name,
            Source = Source,
            CreatedUtc = CreatedUtc,
            Author = Author,
            OwnerId = OwnerId,
            Properties = Properties?.Clone(),
        };
    }
}
