namespace OrchardCore.ContentLocalization.Models;

/// <summary>Tenant behavior when a request resolves to localized content.</summary>
public class ContentRequestCultureProviderSettings
{
    /// <summary>Whether visiting localized content writes its culture to the culture cookie.</summary>
    public bool SetCookie { get; set; }
}
