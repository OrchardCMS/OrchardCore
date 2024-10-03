using OrchardCore.Entities;

namespace OrchardCore.UrlRewriting.Models;

public sealed class RewriteRule : Entity
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Source { get; set; }

    public bool SkipFurtherRules { get; set; }
}
