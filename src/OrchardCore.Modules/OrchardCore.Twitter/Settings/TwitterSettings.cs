using System;

namespace OrchardCore.Twitter.Settings;

public class TwitterSettings
{
    public string ConsumerKey { get; set; }

    /// <summary>
    /// Gets or sets the consumer secret.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via ConsumerSecretSecretName.")]
    public string ConsumerSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the consumer secret.
    /// When set, this takes precedence over the ConsumerSecret property.
    /// </summary>
    public string ConsumerSecretSecretName { get; set; }

    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the access token secret.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Use the Secrets module to store sensitive data and reference it via AccessTokenSecretSecretName.")]
    public string AccessTokenSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the access token secret.
    /// When set, this takes precedence over the AccessTokenSecret property.
    /// </summary>
    public string AccessTokenSecretSecretName { get; set; }
}
