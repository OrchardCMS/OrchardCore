namespace OrchardCore.Secrets;

/// <summary>
/// Performs explicit, provider-scoped operations without exposing secret values to callers.
/// </summary>
public interface ISecretStoreOperations
{
    /// <summary>
    /// Copies and verifies a secret before removing it from the source store.
    /// </summary>
    Task<SecretStoreOperationResult> MoveSecretAsync(string name, string sourceStore, string destinationStore);

    /// <summary>
    /// Moves every secret currently in the source store, reporting each item's outcome.
    /// </summary>
    Task<IReadOnlyList<SecretStoreOperationResult>> MoveAllSecretsAsync(string sourceStore, string destinationStore);
}
