using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DbConnectionValidatorContext
{
    public DbConnectionValidatorContext(ShellSettings shellSettings)
    {
        DatabaseTableOptions = shellSettings.GetDatabaseTableOptions();

        Name = shellSettings.Name;
        DatabaseProvider = shellSettings["DatabaseProvider"];
        ConnectionString = shellSettings["ConnectionString"];
        TablePrefix = shellSettings["TablePrefix"];
        Schema = shellSettings["Schema"];
    }

    public DbConnectionValidatorContext(ShellSettings shellSettings, IDbConnectionInfo dbConnectionInfo)
    {
        DatabaseTableOptions = shellSettings.GetDatabaseTableOptions();

        Name = dbConnectionInfo.Name;
        DatabaseProvider = dbConnectionInfo.DatabaseProvider;
        ConnectionString = dbConnectionInfo.ConnectionString;
        TablePrefix = dbConnectionInfo.TablePrefix;
        Schema = dbConnectionInfo.Schema;
    }

    public DatabaseTableOptions DatabaseTableOptions { get; }

    public string Name { get; }

    public string DatabaseProvider { get; }

    public string ConnectionString { get; }

    public string TablePrefix { get; }

    public string Schema { get; }
}
