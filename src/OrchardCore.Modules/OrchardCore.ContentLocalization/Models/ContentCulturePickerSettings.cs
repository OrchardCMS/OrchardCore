namespace OrchardCore.ContentLocalization.Models;

/// <summary>Tenant behavior when a visitor switches the content culture.</summary>
public class ContentCulturePickerSettings
{
    /// <summary>Whether a missing localized variant falls back to the localized homepage.</summary>
    public bool RedirectToHomepage { get; set; }
    /// <summary>Whether switching to a supported culture writes the culture cookie.</summary>
    public bool SetCookie { get; set; } = true;
}
