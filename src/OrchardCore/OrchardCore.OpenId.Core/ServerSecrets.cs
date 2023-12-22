namespace OrchardCore.OpenId;

public static class ServerSecrets
{
    public const string Purpose = "OrchardCore.OpenId.Server";
    public const string Encryption = $"{Purpose}.Encryption";
    public const string Signing = $"{Purpose}.Signing";
}
