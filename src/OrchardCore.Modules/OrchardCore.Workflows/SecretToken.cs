namespace OrchardCore.Workflows;

public static class SecretToken
{
    public const string Purpose = "OrchardCore.Workflows.Token";
    public const string Encryption = $"{Purpose}.Encryption";
    public const string Signing = $"{Purpose}.Signing";
}
