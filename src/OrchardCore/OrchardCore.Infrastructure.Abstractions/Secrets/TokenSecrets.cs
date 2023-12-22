namespace OrchardCore.Secrets;

public static class TokenSecrets
{
    public const string Purpose = "OrchardCore.Secrets.Token";
    public const string Encryption = $"{Purpose}.Encryption";
    public const string Signing = $"{Purpose}.Signing";
}
