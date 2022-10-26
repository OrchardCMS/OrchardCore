using System;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public class DatabaseTableInfo
{
    public string Schema { get; set; }

    public string DocumentTable { get; set; }

    public string TableNameSeparator { get; set; }

    public string TableNameSeparatorModelValue =>
        !String.IsNullOrEmpty(TableNameSeparator)
        ? TableNameSeparator
        : "NULL";

    public IdentityColumnSize IdentityColumnSize { get; set; }

    public static DatabaseTableInfo Create(ShellSettings shellSettings)
    {
        var options = new DatabaseTableInfo();

        Configure(shellSettings, options);

        return options;
    }

    private static void Configure(ShellSettings shellSettings, DatabaseTableInfo options)
    {
        if (shellSettings == null)
        {
            throw new ArgumentNullException(nameof(shellSettings));
        }

        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (!String.IsNullOrWhiteSpace(shellSettings["Schema"]))
        {
            options.Schema = shellSettings["Schema"].Trim();
        }

        options.DocumentTable = "Document";
        if (!String.IsNullOrWhiteSpace(shellSettings["DocumentTable"]))
        {
            options.DocumentTable = shellSettings["DocumentTable"].Trim();
        }

        options.TableNameSeparator = "_";
        if (!String.IsNullOrWhiteSpace(shellSettings["TableNameSeparator"]))
        {
            options.TableNameSeparator = shellSettings["TableNameSeparator"].Trim();
            if (options.TableNameSeparator == "NULL")
            {
                options.TableNameSeparator = String.Empty;
            }
        }

        if (Enum.TryParse<IdentityColumnSize>(shellSettings["IdentityColumnSize"], out var columnSize))
        {
            options.IdentityColumnSize = columnSize;
        }
    }
}
