namespace OrchardCore.Secrets;

public static class TokenSecret
{
    public const string Namespace = "Secrets.Token";
    public const string Encryption = $"{Namespace}.Encryption";
    public const string Signing = $"{Namespace}.Signing";
}
