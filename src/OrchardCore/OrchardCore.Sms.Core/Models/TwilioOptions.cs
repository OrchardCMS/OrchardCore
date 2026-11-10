namespace OrchardCore.Sms.Models;

/// <summary>
/// Represents the runtime Twilio SMS provider options loaded from site settings.
/// </summary>
public class TwilioOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the provider is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the sending phone number.
    /// </summary>
    public string PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the Twilio account SID.
    /// </summary>
    public string AccountSID { get; set; }

    /// <summary>
    /// Gets or sets the decrypted Twilio auth token.
    /// </summary>
    public string AuthToken { get; set; }
}
