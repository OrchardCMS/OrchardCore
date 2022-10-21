using System;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public class DatabaseTableOptions
{
    public string DocumentTable { get; set; } = "Document";

    public string TablePrefixSeparator { get; set; } = "_";

    public string Schema { get; set; }

    public IdentityColumnSize IdentityColumnType { get; set; }

    public static DatabaseTableOptions Create(ShellSettings shellSettings)
    {
        var options = new DatabaseTableOptions();

        Configure(shellSettings, options);

        return options;
    }

    public static void Configure(ShellSettings shellSettings, DatabaseTableOptions options)
    {
        if (shellSettings == null)
        {
            throw new ArgumentNullException(nameof(shellSettings));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Schema = shellSettings["Schema"];

        // For backward compatibility, if the TablePrefixSeparator isn't set, we use _ as the default value.
        options.TablePrefixSeparator = shellSettings["TablePrefixSeparator"] ?? "_";

        if (!String.IsNullOrWhiteSpace(shellSettings["DocumentTable"]))
        {
            options.DocumentTable = shellSettings["DocumentTable"].Trim();
        }

        if (Enum.TryParse<IdentityColumnSize>(shellSettings["IdentityColumnType"], out var columnType))
        {
            options.IdentityColumnType = columnType;
        }
    }
}
