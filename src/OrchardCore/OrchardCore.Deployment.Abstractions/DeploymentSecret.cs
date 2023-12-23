namespace OrchardCore.Deployment;

public static class DeploymentSecret
{
    public const string Namespace = "Deployment";
    public const string Encryption = $"{Namespace}.Encryption";
    public const string Signing = $"{Namespace}.Signing";
}
