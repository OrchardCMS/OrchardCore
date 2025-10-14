namespace OrchardCore.Redis.Azure;

public class AzureRedisOptions
{
    public AzureRedisAuthType AuthenticationType { get; set; }

    public string[] Scopes { get; set; }

    public string TenantId { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}
