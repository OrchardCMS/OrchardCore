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
}
