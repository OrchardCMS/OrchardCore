using System;
using System.Linq;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class DatabaseTableOptions
{
    public DatabaseTableOptions(ShellSettings shellSettings)
    {
        if (shellSettings == null)
        {
            throw new ArgumentNullException(nameof(shellSettings));
        }

        var documentTable = shellSettings["DocumentTable"];
        var tableNameSeparator = shellSettings["TableNameSeparator"];
        var identityColumnSize = shellSettings["IdentityColumnSize"];

        if (shellSettings.VersionId == null)
        {
            documentTable ??= shellSettings["DefaultDocumentTable"];
            tableNameSeparator ??= shellSettings["DefaultTableNameSeparator"];
            identityColumnSize ??= shellSettings["DefaultIdentityColumnSize"];
        }

        Initialize(documentTable, tableNameSeparator, identityColumnSize);
    }

    public virtual string DocumentTable { get; private set; }

    public virtual string TableNameSeparator { get; private set; }

    public virtual string IdentityColumnSize { get; private set; }

    private void Initialize(string documentTable, string tableNameSeparator, string identityColumnSize)
    {
        DocumentTable = "Document";
        if (!String.IsNullOrWhiteSpace(documentTable))
        {
            documentTable= documentTable.Trim();
            if (documentTable.Any(c => !Char.IsLetterOrDigit(c)))
            {
                throw new InvalidOperationException("The 'Document' table name configuration should only contain alpha-numeric chars.");
            }

            DocumentTable = documentTable;
        }

        TableNameSeparator = "_";
        if (!String.IsNullOrWhiteSpace(tableNameSeparator))
        {

            tableNameSeparator = tableNameSeparator.Trim();
            if (tableNameSeparator == "NULL")
            {
                tableNameSeparator = String.Empty;
            }
            else if (tableNameSeparator.Any(c => c != '_'))
            {
                throw new InvalidOperationException("The 'TableNameSeparator' configuration should only contain underscores.");
            }

            TableNameSeparator = tableNameSeparator;
        }

        IdentityColumnSize = nameof(Int32);
        if (!String.IsNullOrWhiteSpace(identityColumnSize))
        {
            identityColumnSize = identityColumnSize.Trim();
            if (identityColumnSize != nameof(Int32) && identityColumnSize != nameof(Int64))
            {
                throw new InvalidOperationException("The 'IdentityColumnSize' configuration should be 'Int32' or 'Int64'.");
            }

            IdentityColumnSize = identityColumnSize;
        }
    }

    public static void PresetDefaultValues(ShellSettings settings)
    {
        if (settings.VersionId == null)
        {
            settings["DocumentTable"] ??= settings["DefaultDocumentTable"];
            settings["TableNameSeparator"] ??= settings["DefaultTableNameSeparator"];
            settings["IdentityColumnSize"] ??= settings["DefaultIdentityColumnSize"];
        }
    }
}
