namespace OrchardCore.Secrets;

/// <summary>
/// Represents a secret store that provides storage and retrieval of secrets.
/// Multiple implementations can exist (database, Azure Key Vault, etc.).
/// </summary>
public interface ISecretStore
{
    /// <summary>
    /// Gets the unique name of this secret store.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets whether this store is read-only.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Gets a secret by its name.
    /// </summary>
    /// <typeparam name="T">The type of secret to retrieve.</typeparam>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret if found, otherwise null.</returns>
    Task<T> GetSecretAsync<T>(string name) where T : class, ISecret;

    /// <summary>
    /// Saves a secret with the specified name.
    /// </summary>
    /// <typeparam name="T">The type of secret to save.</typeparam>
    /// <param name="name">The name of the secret.</param>
    /// <param name="secret">The secret to save.</param>
    Task SaveSecretAsync<T>(string name, T secret) where T : class, ISecret;

    /// <summary>
    /// Removes a secret by its name.
    /// </summary>
    /// <param name="name">The name of the secret to remove.</param>
    Task RemoveSecretAsync(string name);

    /// <summary>
    /// Gets information about all secrets in this store.
    /// </summary>
    /// <returns>A collection of secret information entries.</returns>
    Task<IEnumerable<SecretInfo>> GetSecretInfosAsync();
}
