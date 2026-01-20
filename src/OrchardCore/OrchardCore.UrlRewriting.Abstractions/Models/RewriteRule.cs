using OrchardCore.Entities;

namespace OrchardCore.UrlRewriting.Models;

public sealed class RewriteRule : Entity
{
    public string Id { get; set; }

    public string Source { get; set; }

    public string Name { get; set; }

    public int Order { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string OwnerId { get; set; }

    public string Author { get; set; }

    public RewriteRule Clone()
    {
        return new RewriteRule
        {
            Id = Id,
            Source = Source,
            Name = Name,
            Order = Order,
            CreatedUtc = CreatedUtc,
            OwnerId = OwnerId,
            Author = Author,
            Properties = Properties,
        };
    }
}
