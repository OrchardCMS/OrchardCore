namespace OrchardCore.UrlRewriting.Models;

public class UrlRedirectSourceMetadata
{
    public string Pattern { get; set; }

    public string SubstitutionPattern { get; set; }

    public bool IsCaseInsensitive { get; set; }

    public bool AppendQueryString { get; set; }

    public RedirectType RedirectType { get; set; }
}
