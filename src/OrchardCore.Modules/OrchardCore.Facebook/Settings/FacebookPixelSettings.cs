namespace OrchardCore.Facebook.Settings;

public class FacebookPixelSettings
{
    public string PixelId { get; set; }

    /// <summary>
    /// The Meta Conversions API access token, generated from the Events Manager for the pixel's
    /// dataset. Stored encrypted via <see cref="Microsoft.AspNetCore.DataProtection.IDataProtectionProvider"/>.
    /// </summary>
    public string ConversionsApiAccessToken { get; set; }

    /// <summary>
    /// Optional test event code from the Events Manager "Test Events" tab, sent with every
    /// Conversions API request so events show up in the test console instead of production data.
    /// </summary>
    public string ConversionsApiTestEventCode { get; set; }
}
