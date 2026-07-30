namespace OrchardCore.Tenants;

public class TenantsOptions
{
    public bool TenantRemovalAllowed { get; set; }

    public bool RequireTablePrefix { get; set; }

    public string TablePrefixPattern { get; set; }

    public string SchemaPattern { get; set; }
}
