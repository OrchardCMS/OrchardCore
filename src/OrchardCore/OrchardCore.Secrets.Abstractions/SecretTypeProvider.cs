using System.Text.Json;

namespace OrchardCore.Secrets;

/// <summary>
/// Base implementation of <see cref="ISecretTypeProvider"/> for a specific secret type.
/// </summary>
/// <typeparam name="TSecret">The secret type.</typeparam>
public abstract class SecretTypeProvider<TSecret> : ISecretTypeProvider where TSecret : class, ISecret, new()
{
    public virtual string Name => typeof(TSecret).Name;

    public abstract string DisplayName { get; }

    public abstract string Description { get; }

    public Type SecretType => typeof(TSecret);

    public ISecret Create() => new TSecret();

    public virtual string Serialize(ISecret secret) => JsonSerializer.Serialize(secret, SecretType);

    public virtual ISecret Deserialize(string json) =>
        JsonSerializer.Deserialize<TSecret>(json) ?? throw new JsonException($"The payload for secret type '{Name}' is null.");
}
