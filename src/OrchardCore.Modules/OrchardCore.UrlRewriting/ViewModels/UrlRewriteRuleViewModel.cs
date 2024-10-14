namespace OrchardCore.UrlRewriting.ViewModels;

public class UrlRewriteRuleViewModel
{
    public string Pattern { get; set; }

    public string SubstitutionPattern { get; set; }

    public bool IsCaseInsensitive { get; set; }

    public bool AppendQueryString { get; set; }

    public bool SkipFurtherRules { get; set; }
}
