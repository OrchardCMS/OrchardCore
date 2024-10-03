using OrchardCore.Entities;

namespace OrchardCore.UrlRewriting.Models;

public sealed class RewriteRule : Entity
{
    public string Id { get; set; }

    public string Name { get; set; }

    public string Source { get; set; }

    public string Pattern { get; set; }

    public string Substitution { get; set; }

    public string Flags { get; set; }
    public bool SkipFurtherRules { get; set; }

    public string[] GetFlagsCollection()
    {
        return Flags?.Split(',') ?? [];
    }

    public RewriteRule Clone()
    {
        return new RewriteRule
        {
            Name = Name,
            Pattern = Pattern,
            Substitution = Substitution,
            Flags = Flags
        };
    }
}
