namespace OrchardCore.UrlRewriting.ViewModels;

public class UrlRewriteRuleViewModel
{
    public string Pattern { get; set; }

    public string SubstitutionUrl { get; set; }

    public bool IgnoreCase { get; set; }

    public bool AppendQueryString { get; set; }

    public bool SkipFurtherRules { get; set; }
}
