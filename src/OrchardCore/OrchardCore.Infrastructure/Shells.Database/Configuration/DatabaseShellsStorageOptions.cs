using OrchardCore.Data;

namespace OrchardCore.Shells.Database.Configuration;

public class DatabaseShellsStorageOptions : IDbConnectionInfo
{
    public bool MigrateFromFiles { get; set; }
    public string DatabaseProvider { get; set; }
    public string ConnectionString { get; set; }
    public string TablePrefix { get; set; }
    public string Schema { get; set; }
}
