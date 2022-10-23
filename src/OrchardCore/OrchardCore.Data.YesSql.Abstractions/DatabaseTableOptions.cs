using System;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public class DatabaseTableOptions
{
    public string DocumentTable { get; set; }

    public string TableNameSeparator { get; set; }

    public string Schema { get; set; }

    public IdentityColumnSize IdentityColumnSize { get; set; }

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

        // For backward compatibility, if the TableNameSeparator isn't set, we use "_" as the default value.
        options.TableNameSeparator = shellSettings["TableNameSeparator"] ?? "_";
        if (options.TableNameSeparator == "NULL")
        {
            options.TableNameSeparator = String.Empty;
        }

        options.DocumentTable = "Document";

        if (!String.IsNullOrWhiteSpace(shellSettings["DocumentTable"]))
        {
            options.DocumentTable = shellSettings["DocumentTable"].Trim();
        }

        if (Enum.TryParse<IdentityColumnSize>(shellSettings["IdentityColumnSize"], out var columnType))
        {
            options.IdentityColumnSize = columnType;
        }
    }
}
