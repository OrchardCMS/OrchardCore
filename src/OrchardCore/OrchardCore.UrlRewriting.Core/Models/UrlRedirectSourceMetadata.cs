namespace OrchardCore.UrlRewriting.Models;

public class UrlRedirectSourceMetadata
{
    public string Pattern { get; set; }

    public bool IgnoreCase { get; set; }

    public string SubstitutionUrl { get; set; }

    public bool AppendQueryString { get; set; }

    public RedirectType RedirectType { get; set; }
}

public enum RedirectType
{
    Found302,
    MovedPermanently301,
    TemporaryRedirect307,
    PermanentRedirect308,
}
