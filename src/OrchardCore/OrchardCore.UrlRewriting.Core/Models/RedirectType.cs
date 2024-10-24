namespace OrchardCore.UrlRewriting.Models;

public enum RedirectType
{
    Found = 302,
    MovedPermanently = 301,
    TemporaryRedirect = 307,
    PermanentRedirect = 308,
}
