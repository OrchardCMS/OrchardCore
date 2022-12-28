using System;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DbConnectionValidatorContext
{
    public DbConnectionValidatorContext(DatabaseTableOptions options)
    {
        TableOptions = options ?? throw new ArgumentNullException(nameof(options));
    }

    public DbConnectionValidatorContext(ShellSettings shellSettings)
    {
        TableOptions = new DatabaseTableOptions(shellSettings);
    }

    public string DatabaseProvider { get; set; }

    public string ConnectionString { get; set; }

    public string TablePrefix { get; set; }

    public string Schema { get; set; }

    public string ShellName { get; set; }

    public DatabaseTableOptions TableOptions { get; }
}
