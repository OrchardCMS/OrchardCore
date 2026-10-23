namespace OrchardCore.Facebook.Settings;

public class FacebookSettings
{
    public string AppId { get; set; }

    /// <summary>
    /// Gets or sets the app secret, protected with Data Protection.
    /// </summary>
    public string AppSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the app secret.
    /// When set, this takes precedence over <see cref="AppSecret"/>.
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
