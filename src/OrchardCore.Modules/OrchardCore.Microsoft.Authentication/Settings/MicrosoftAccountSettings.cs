using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Microsoft.Authentication.Settings;

public class MicrosoftAccountSettings
{
    public string AppId { get; set; }

    /// <summary>
    /// Gets or sets the app secret.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via AppSecretSecretName.")]
    public string AppSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the Microsoft app secret.
    /// When set, this takes precedence over the AppSecret property.
    /// </summary>
    public string AppSecretSecretName { get; set; }

    public PathString CallbackPath { get; set; }
    public bool SaveTokens { get; set; }
}
