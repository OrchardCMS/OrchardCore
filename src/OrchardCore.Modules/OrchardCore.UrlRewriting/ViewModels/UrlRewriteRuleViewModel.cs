namespace OrchardCore.UrlRewriting.ViewModels;

public class UrlRewriteRuleViewModel
{
    public string SubstitutionUrl { get; set; }

    public bool AppendQueryString { get; set; }

    public string Pattern { get; set; }

    public bool IgnoreCase { get; set; }

    public bool SkipFurtherRules { get; set; }
}
