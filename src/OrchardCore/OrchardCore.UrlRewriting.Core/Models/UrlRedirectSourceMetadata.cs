namespace OrchardCore.UrlRewriting.Models;

public class UrlRedirectSourceMetadata
{
    public string Pattern { get; set; }

    public string SubstitutionUrl { get; set; }

    public bool IgnoreCase { get; set; }

    public bool AppendQueryString { get; set; }

    public RedirectType RedirectType { get; set; }
}

public enum RedirectType
{
    Found = 302,
    MovedPermanently = 301,
    TemporaryRedirect = 307,
    PermanentRedirect = 308,
}
