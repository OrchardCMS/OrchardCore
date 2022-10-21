using System;

namespace OrchardCore.Data;

public class DbConnectionValidatorContext
{
    public string DatabaseProvider { get; set; }

    public string ConnectionString { get; set; }

    public string TablePrefix { get; set; }

    public string ShellName { get; set; }

    public DatabaseTableOptions TableOptions { get; }

    public DbConnectionValidatorContext(DatabaseTableOptions options)
    {
        TableOptions = options ?? throw new ArgumentNullException(nameof(options));
    }
}
