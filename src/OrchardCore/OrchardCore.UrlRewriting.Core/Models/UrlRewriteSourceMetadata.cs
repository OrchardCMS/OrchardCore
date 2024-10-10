namespace OrchardCore.UrlRewriting.Models;

public class UrlRewriteSourceMetadata
{
    public string Pattern { get; set; }

    public bool IgnoreCase { get; set; }

    public string Url { get; set; }

    public bool AppendQueryString { get; set; }

    public bool SkipFurtherRules { get; set; }
}
