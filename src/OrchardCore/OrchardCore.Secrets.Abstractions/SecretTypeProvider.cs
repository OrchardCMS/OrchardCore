using System.Text.Json;

namespace OrchardCore.Secrets;

/// <summary>
/// Base implementation of <see cref="ISecretTypeProvider"/> for a specific secret type.
/// </summary>
/// <typeparam name="TSecret">The secret type, one of the cases of <see cref="Secret"/>.</typeparam>
public abstract class SecretTypeProvider<TSecret> : ISecretTypeProvider where TSecret : class, new()
{
    public virtual string Name => typeof(TSecret).Name;

    public abstract string DisplayName { get; }

    public abstract string Description { get; }

    public Type SecretType => typeof(TSecret);

    public Secret Create() => Secret.FromValue(new TSecret());

    public virtual string Serialize(Secret secret)
    {
        if (secret.Value is not TSecret value)
        {
            throw new ArgumentException($"The secret is not a '{Name}'.", nameof(secret));
        }

        return JsonSerializer.Serialize(value);
    }

    public virtual Secret Deserialize(string json) =>
        Secret.FromValue(JsonSerializer.Deserialize<TSecret>(json) ?? throw new JsonException($"The payload for secret type '{Name}' is null."));
}
