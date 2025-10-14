namespace OrchardCore.Redis.Azure;

public enum AzureRedisAuthType
{
    DefaultAzureCredential,
    ManagedIdentity,
    ClientSecret,
}
