namespace OrchardCore.Tenants.Services;

public sealed class TenantDatabasePatternResolution
{
    public bool HasTablePrefixPattern { get; set; }

    public string TablePrefix { get; set; }

    public string TablePrefixError { get; set; }

    public bool HasSchemaPattern { get; set; }

    public string Schema { get; set; }

    public string SchemaError { get; set; }
}
