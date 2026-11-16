namespace OrchardCore.Secrets;

/// <summary>
/// Extension methods for <see cref="ISecretStore"/>.
/// </summary>
public static class SecretStoreExtensions
{
    /// <summary>
    /// Gets a secret of a specific type by its name.
    /// </summary>
    /// <typeparam name="T">The type of the secret, one of the cases of <see cref="Secret"/>.</typeparam>
    /// <param name="secretStore">The <see cref="ISecretStore"/>.</param>
    /// <param name="name">The name of the secret.</param>
    /// <returns>The secret if found and of type <typeparamref name="T"/>, otherwise <see langword="null"/>.</returns>
    public static async Task<T> GetSecretAsync<T>(this ISecretStore secretStore, string name)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(secretStore);

        return (await secretStore.GetSecretAsync(name)).Value as T;
    }
}
