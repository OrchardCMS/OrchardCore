namespace OrchardCore.UrlRewriting.Models;

public class RewriteRule
{
    public string Name { get; set; }

    public string Pattern { get; set; }

    public string Substitution { get; set; }

    public string Flags { get; set; }

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
