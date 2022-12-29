using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DbConnectionValidatorContext
{
    public DbConnectionValidatorContext(ShellSettings shellSettings)
    {
        TableOptions = shellSettings.GetDatabaseTableOptions();
        DatabaseProvider = shellSettings["DatabaseProvider"];
        ConnectionString = shellSettings["ConnectionString"];
        TablePrefix = shellSettings["TablePrefix"];
        Schema = shellSettings["Schema"];
        Name = shellSettings.Name;
    }

    public DbConnectionValidatorContext(ShellSettings shellSettings, IShellDatabaseInfo shellDatabaseInfo)
    {
        TableOptions = shellSettings.GetDatabaseTableOptions();
        DatabaseProvider = shellDatabaseInfo.DatabaseProvider;
        ConnectionString = shellDatabaseInfo.ConnectionString;
        TablePrefix = shellDatabaseInfo.TablePrefix;
        Schema = shellDatabaseInfo.Schema;
        Name = shellDatabaseInfo.Name;
    }

    public DatabaseTableOptions TableOptions { get; }

    public string DatabaseProvider { get; }

    public string ConnectionString { get; }

    public string TablePrefix { get; }

    public string Schema { get; }

    public string Name { get; }
}
