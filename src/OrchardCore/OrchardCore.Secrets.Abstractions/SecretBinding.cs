namespace OrchardCore.Secrets;

/// <summary>
/// Provides a binding between a secret name and a specific secret store.
/// Used to configure which secrets are stored in which stores.
/// </summary>
public class SecretBinding
{
    /// <summary>
    /// Gets or sets the name of the secret.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the store where this secret is stored.
    /// </summary>
    public string Store { get; set; }

    /// <summary>
    /// Gets or sets a description of the secret.
    /// </summary>
    public string Description { get; set; }
}
