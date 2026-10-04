namespace OrchardCore.Secrets;

/// <summary>
/// Metadata-only outcome of a store operation. Never contains secret values or storage exceptions.
/// </summary>
public sealed class SecretStoreOperationResult
{
    public string Name { get; init; }
    public bool Succeeded { get; init; }
    public bool DestinationSaved { get; init; }
    public SecretStoreOperationFailure Failure { get; init; }
}

public enum SecretStoreOperationFailure
{
    None,
    SourceMissing,
    DestinationCollision,
    UnsupportedType,
    VerificationFailed,
    SourceChanged,
    StorageFailure,
    SourceRemovalFailed,
    OperationTimedOut,
}
