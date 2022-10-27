using System;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public class DatabaseTableOptions
{
    public DatabaseTableOptions(ShellSettings shellSettings)
    {
        if (shellSettings == null)
        {
            throw new ArgumentNullException(nameof(shellSettings));
        }

        _ = Enum.TryParse<IdentityColumnSize>(shellSettings["IdentityColumnSize"], out var identityColumnSize);

        Initialize(
            shellSettings?["Schema"],
            shellSettings["DocumentTable"],
            shellSettings["TableNameSeparator"],
            identityColumnSize);
    }

    public DatabaseTableOptions(
        string schema,
        string documentTable,
        string tableNameSeparator,
        IdentityColumnSize identityColumnSize) =>
            Initialize(schema, documentTable, tableNameSeparator, identityColumnSize);

    public virtual string Schema { get; private set; }

    public virtual string DocumentTable { get; private set; }

    public virtual string TableNameSeparator { get; private set; }

    public virtual IdentityColumnSize IdentityColumnSize { get; private set; }

    private void Initialize(
        string schema,
        string documentTable,
        string tableNameSeparator,
        IdentityColumnSize identityColumnSize)
    {
        if (!String.IsNullOrWhiteSpace(schema))
        {
            Schema = schema.Trim();
        }

        DocumentTable = "Document";
        if (!String.IsNullOrWhiteSpace(documentTable))
        {
            DocumentTable = documentTable.Trim();
        }

        TableNameSeparator = "_";
        if (!String.IsNullOrWhiteSpace(tableNameSeparator))
        {
            TableNameSeparator = tableNameSeparator.Trim();
            if (TableNameSeparator == "NULL")
            {
                TableNameSeparator = String.Empty;
            }
        }

        IdentityColumnSize = IdentityColumnSize.Int32;
        if (Enum.IsDefined(identityColumnSize))
        {
            IdentityColumnSize = identityColumnSize;
        }
    }
}
