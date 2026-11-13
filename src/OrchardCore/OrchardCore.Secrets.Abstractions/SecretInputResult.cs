namespace OrchardCore.Secrets;

/// <summary>
/// The values to store in the settings after a <see cref="SecretInputViewModel"/> was applied.
/// </summary>
public sealed class SecretInputResult
{
    /// <summary>
    /// Gets whether the posted values were valid. When <see langword="false"/>, the settings should not be changed.
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// Gets the protected value to keep in the settings, or <see langword="null"/> when the settings reference a secret.
    /// </summary>
    public string ProtectedValue { get; init; }

    /// <summary>
    /// Gets the name of the secret the settings reference, or <see langword="null"/> when they keep a protected value.
    /// </summary>
    public string SecretName { get; init; }
}
