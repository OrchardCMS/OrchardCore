using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DbConnectionValidatorContext : IDbConnectionInfo
{
    public DbConnectionValidatorContext(ShellSettings shellSettings)
    {
        ShellName = shellSettings.Name;
        DatabaseProvider = shellSettings["DatabaseProvider"];
        ConnectionString = shellSettings["ConnectionString"];
        TablePrefix = shellSettings["TablePrefix"];
        Schema = shellSettings["Schema"];

        TableOptions = shellSettings.GetDatabaseTableOptions();
    }

    public DbConnectionValidatorContext(ShellSettings shellSettings, IDbConnectionInfo dbConnectionInfo)
    {
        ShellName = shellSettings.Name;
        DatabaseProvider = dbConnectionInfo.DatabaseProvider;
        ConnectionString = dbConnectionInfo.ConnectionString;
        TablePrefix = dbConnectionInfo.TablePrefix;
        Schema = dbConnectionInfo.Schema;

        TableOptions = shellSettings.GetDatabaseTableOptions();
    }

    public string ShellName { get; }

    public string DatabaseProvider { get; }

    public string ConnectionString { get; }

    public string TablePrefix { get; }

    public string Schema { get; }

    public DatabaseTableOptions TableOptions { get; }
}
