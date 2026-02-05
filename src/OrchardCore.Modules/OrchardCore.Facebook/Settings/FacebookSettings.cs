using System;

namespace OrchardCore.Facebook.Settings;

public class FacebookSettings
{
    public string AppId { get; set; }

    /// <summary>
    /// Gets or sets the app secret.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via AppSecretSecretName.")]
    public string AppSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the Facebook app secret.
    /// When set, this takes precedence over the AppSecret property.
    /// </summary>
    public string AppSecretSecretName { get; set; }

    public bool FBInit { get; set; }

    public string FBInitParams { get; set; } = """
        status: true,
        xfbml: true,
        autoLogAppEvents: true
        """;

    public string SdkJs { get; set; } = "sdk.js";
    public string Version { get; set; } = "v3.2";
}
