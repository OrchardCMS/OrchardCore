using System;

namespace OrchardCore.Sms.Models;

public class TwilioSettings
{
    public bool IsEnabled { get; set; }

    public string PhoneNumber { get; set; }

    public string AccountSID { get; set; }

    /// <summary>
    /// Gets or sets the auth token.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via AuthTokenSecretName.")]
    public string AuthToken { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the Twilio auth token.
    /// When set, this takes precedence over the AuthToken property.
    /// </summary>
    public string AuthTokenSecretName { get; set; }
}
