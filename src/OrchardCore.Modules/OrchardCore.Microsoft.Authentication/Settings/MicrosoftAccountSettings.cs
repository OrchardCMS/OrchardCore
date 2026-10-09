using Microsoft.AspNetCore.Http;

namespace OrchardCore.Microsoft.Authentication.Settings;

public class MicrosoftAccountSettings
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

    public PathString CallbackPath { get; set; }
    public bool SaveTokens { get; set; }
}
