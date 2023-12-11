namespace OrchardCore.Deployment.Remote;

public static class Secrets
{
    public const string Namespace = "OrchardCore.Deployment.Remote";
    public const string Encryption = $"{Namespace}.EncryptionSecret";
    public const string Signing = $"{Namespace}.SigningSecret";
    public const string ApiKey = $"{Namespace}.ApiKey";
}
