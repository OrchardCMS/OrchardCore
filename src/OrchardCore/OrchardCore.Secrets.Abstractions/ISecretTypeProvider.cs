namespace OrchardCore.Secrets;

/// <summary>
/// Provides information about a secret type.
/// </summary>
public interface ISecretTypeProvider
{
    /// <summary>
    /// Gets the name of the secret type.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the display name of the secret type.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of the secret type.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the CLR type of the secret.
    /// </summary>
    Type SecretType { get; }

    /// <summary>
    /// Creates a new instance of the secret.
    /// </summary>
    ISecret Create();
}
