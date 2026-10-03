using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Google.Authentication.Settings;

public class GoogleAuthenticationSettings
{
    public string ClientID { get; set; }

    /// <summary>
    /// Gets or sets the client secret.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via ClientSecretSecretName.")]
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the Google client secret.
    /// When set, this takes precedence over the ClientSecret property.
    /// </summary>
    public string ClientSecretSecretName { get; set; }

    public PathString CallbackPath { get; set; }
    public bool SaveTokens { get; set; }
}
