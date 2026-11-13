namespace OrchardCore.Secrets;

/// <summary>
/// Extension methods for <see cref="ISecretManager"/>.
/// </summary>
public static class SecretManagerExtensions
{
    /// <summary>
    /// Gets a secret of a specific type by its name, from the first configured store containing it.
    /// </summary>
    /// <typeparam name="T">The type of the secret, one of the cases of <see cref="Secret"/>.</typeparam>
    /// <param name="secretManager">The <see cref="ISecretManager"/>.</param>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret if found and of type <typeparamref name="T"/>, otherwise <see langword="null"/>.</returns>
    public static async Task<T> GetSecretAsync<T>(this ISecretManager secretManager, string name)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(secretManager);

        return (await secretManager.GetSecretAsync(name)).Value as T;
    }

    /// <summary>
    /// Gets a secret of a specific type by its name, from a specific store.
    /// </summary>
    /// <typeparam name="T">The type of the secret, one of the cases of <see cref="Secret"/>.</typeparam>
    /// <param name="secretManager">The <see cref="ISecretManager"/>.</param>
    /// <param name="name">The name of the secret.</param>
    /// <param name="storeName">The name of the store to search.</param>
    /// <returns>The secret if found and of type <typeparamref name="T"/>, otherwise <see langword="null"/>.</returns>
    public static async Task<T> GetSecretAsync<T>(this ISecretManager secretManager, string name, string storeName)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(secretManager);

        return (await secretManager.GetSecretAsync(name, storeName)).Value as T;
    }
}
