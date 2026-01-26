namespace OrchardCore.Secrets;

/// <summary>
/// Manages secrets across multiple secret stores, providing a unified interface
/// for retrieving and storing secrets.
/// </summary>
public interface ISecretManager
{
    /// <summary>
    /// Gets a secret by its name, searching across all configured stores.
    /// </summary>
    /// <typeparam name="T">The type of secret to retrieve.</typeparam>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret if found, otherwise null.</returns>
    Task<T> GetSecretAsync<T>(string name) where T : class, ISecret;

    /// <summary>
    /// Gets a secret by its name from a specific store.
    /// </summary>
    /// <typeparam name="T">The type of secret to retrieve.</typeparam>
    /// <param name="name">The name of the secret.</param>
    /// <param name="storeName">The name of the store to search.</param>
    /// <returns>The secret if found, otherwise null.</returns>
    Task<T> GetSecretAsync<T>(string name, string storeName) where T : class, ISecret;

    /// <summary>
    /// Saves a secret with the specified name to the default writable store.
    /// </summary>
    /// <typeparam name="T">The type of secret to save.</typeparam>
    /// <param name="name">The name of the secret.</param>
    /// <param name="secret">The secret to save.</param>
    Task SaveSecretAsync<T>(string name, T secret) where T : class, ISecret;

    /// <summary>
    /// Saves a secret with the specified name to a specific store.
    /// </summary>
    /// <typeparam name="T">The type of secret to save.</typeparam>
    /// <param name="name">The name of the secret.</param>
    /// <param name="secret">The secret to save.</param>
    /// <param name="storeName">The name of the store to save to.</param>
    Task SaveSecretAsync<T>(string name, T secret, string storeName) where T : class, ISecret;

    /// <summary>
    /// Removes a secret by its name from all stores.
    /// </summary>
    /// <param name="name">The name of the secret to remove.</param>
    Task RemoveSecretAsync(string name);

    /// <summary>
    /// Removes a secret by its name from a specific store.
    /// </summary>
    /// <param name="name">The name of the secret to remove.</param>
    /// <param name="storeName">The name of the store to remove from.</param>
    Task RemoveSecretAsync(string name, string storeName);

    /// <summary>
    /// Gets information about all secrets across all stores.
    /// </summary>
    /// <returns>A collection of secret information entries.</returns>
    Task<IEnumerable<SecretInfo>> GetSecretInfosAsync();

    /// <summary>
    /// Gets all configured secret stores.
    /// </summary>
    /// <returns>A collection of secret stores.</returns>
    IEnumerable<ISecretStore> GetStores();
}
