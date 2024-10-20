namespace OrchardCore.UrlRewriting.Models;

public sealed class UrlRewriteSourceMetadata
{
    public string Pattern { get; set; }

    public bool IsCaseInsensitive { get; set; }

    public string SubstitutionPattern { get; set; }

    public QueryStringPolicy QueryStringPolicy { get; set; }

    public bool SkipFurtherRules { get; set; }
}
