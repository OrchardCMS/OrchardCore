namespace OrchardCore.OpenId;

public static class ServerSecret
{
    public const string Namespace = "OpenId.Server";
    public const string Encryption = $"{Namespace}.Encryption";
    public const string Signing = $"{Namespace}.Signing";

    public const string X509Namespace = "OpenId.Server.X509";
    public const string X509Encryption = $"{X509Namespace}.Encryption";
    public const string X509Signing = $"{X509Namespace}.Signing";
}
