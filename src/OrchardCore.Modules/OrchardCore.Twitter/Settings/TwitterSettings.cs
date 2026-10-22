namespace OrchardCore.Twitter.Settings;

public class TwitterSettings
{
    public string ConsumerKey { get; set; }

    /// <summary>
    /// Gets or sets the consumer secret, protected with Data Protection.
    /// </summary>
    public string ConsumerSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the consumer secret.
    /// When set, this takes precedence over <see cref="ConsumerSecret"/>.
    /// </summary>
    public string ConsumerSecretSecretName { get; set; }

    public string AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the access token secret, protected with Data Protection.
    /// </summary>
    public string AccessTokenSecret { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret of the Secrets module containing the access token secret.
    /// When set, this takes precedence over <see cref="AccessTokenSecret"/>.
    /// </summary>
    public string AccessTokenSecretSecretName { get; set; }
}
