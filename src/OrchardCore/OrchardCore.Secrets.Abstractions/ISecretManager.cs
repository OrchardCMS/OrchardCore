namespace OrchardCore.Secrets;

/// <summary>
/// Manages secrets across multiple secret stores, providing a unified interface
/// for retrieving and storing secrets.
/// </summary>
public interface ISecretManager
{
    /// <summary>
    /// Gets a secret by its name, from the first configured store containing it.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret if found, otherwise a secret with a <see langword="null"/> <see cref="Secret.Value"/>.</returns>
    /// <remarks>
    /// Use <see cref="SecretManagerExtensions.GetSecretAsync{T}(ISecretManager, string)"/> to get a secret of a specific type.
    /// </remarks>
    Task<Secret> GetSecretAsync(string name);

    /// <summary>
    /// Gets a secret by its name from a specific store.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="storeName">The name of the store to search.</param>
    /// <returns>The secret if found, otherwise a secret with a <see langword="null"/> <see cref="Secret.Value"/>.</returns>
    Task<Secret> GetSecretAsync(string name, string storeName);

    /// <summary>
    /// Saves a secret with the specified name to the default writable store.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="secret">The secret to save.</param>
    /// <param name="options">Optional save options including description and expiration.</param>
    Task SaveSecretAsync(string name, Secret secret, SecretSaveOptions options = null);

    /// <summary>
    /// Saves a secret with the specified name to a specific store.
    /// </summary>
    /// <param name="name">The name of the secret.</param>
    /// <param name="secret">The secret to save.</param>
    /// <param name="storeName">The name of the store to save to.</param>
    /// <param name="options">Optional save options including description and expiration.</param>
    Task SaveSecretAsync(string name, Secret secret, string storeName, SecretSaveOptions options = null);

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
