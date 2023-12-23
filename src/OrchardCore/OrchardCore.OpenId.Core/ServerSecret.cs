namespace OrchardCore.OpenId;

public static class ServerSecret
{
    public const string Namespace = "OpenId.Server";
    public const string Encryption = $"{Namespace}.Encryption";
    public const string Signing = $"{Namespace}.Signing";
}
