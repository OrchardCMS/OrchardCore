namespace OrchardCore.UrlRewriting.Models;

public sealed class UrlRedirectSourceMetadata
{
    public string Pattern { get; set; }

    public string SubstitutionPattern { get; set; }

    public bool IsCaseInsensitive { get; set; }

    public QueryStringPolicy QueryStringPolicy { get; set; }

    public RedirectType RedirectType { get; set; }
}
