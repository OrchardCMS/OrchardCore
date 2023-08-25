namespace OrchardCore.Secrets;

public class SecretFactory<TSecret> : ISecretFactory where TSecret : Secret, new()
{
    private static readonly string _typeName = typeof(TSecret).Name;

    public string Name => _typeName;

    public Secret Create() => new TSecret();
}
