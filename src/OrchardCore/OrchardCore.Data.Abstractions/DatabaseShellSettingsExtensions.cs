using System;
using System.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Data;

public static class DatabaseShellSettingsExtensions
{
    public static ShellSettings WithDefaultDatabaseTableOptions(this ShellSettings shellSettings)
    {
        if (shellSettings.State == TenantState.Uninitialized ||
            shellSettings.State == TenantState.Initializing)
        {
            shellSettings["DocumentTable"] ??= shellSettings["DefaultDocumentTable"];
            shellSettings["TableNameSeparator"] ??= shellSettings["DefaultTableNameSeparator"];
            shellSettings["IdentityColumnSize"] ??= shellSettings["DefaultIdentityColumnSize"];
        }

        return shellSettings;
    }

    public static string GetDocumentTableName(this ShellSettings shellSettings)
    {
        var documentTable = shellSettings["DocumentTable"];
        if (shellSettings.State == TenantState.Uninitialized ||
            shellSettings.State == TenantState.Initializing)
        {
            documentTable ??= shellSettings["DefaultDocumentTable"];
        }

        if (String.IsNullOrWhiteSpace(documentTable))
        {
            documentTable = "Document";
        }
        else
        {
            documentTable = documentTable.Trim();
            if (documentTable.Any(c => !Char.IsLetterOrDigit(c)))
            {
                throw new InvalidOperationException("The configured 'DocumentTable' name should only contain alpha-numeric chars.");
            }
        }

        return documentTable;
    }

    public static string GetTableNameSeparator(this ShellSettings shellSettings)
    {
        var tableNameSeparator = shellSettings["TableNameSeparator"];
        if (shellSettings.State == TenantState.Uninitialized ||
            shellSettings.State == TenantState.Initializing)
        {
            tableNameSeparator ??= shellSettings["DefaultTableNameSeparator"];
        }

        if (String.IsNullOrWhiteSpace(tableNameSeparator))
        {
            tableNameSeparator = "_";
        }
        else
        {
            tableNameSeparator = tableNameSeparator.Trim();
            if (tableNameSeparator == "NULL")
            {
                tableNameSeparator = String.Empty;
            }
            else if (tableNameSeparator.Any(c => c != '_'))
            {
                throw new InvalidOperationException("The configured 'TableNameSeparator' should only contain underscores.");
            }
        }

        return tableNameSeparator;
    }

    public static string GetIdentityColumnSize(this ShellSettings shellSettings)
    {
        var identityColumnSize = shellSettings["IdentityColumnSize"];
        if (shellSettings.State == TenantState.Uninitialized ||
            shellSettings.State == TenantState.Initializing)
        {
            identityColumnSize ??= shellSettings["DefaultIdentityColumnSize"];
        }

        if (String.IsNullOrWhiteSpace(identityColumnSize))
        {
            identityColumnSize = nameof(Int32);
        }
        else
        {
            identityColumnSize = identityColumnSize.Trim();
            if (identityColumnSize != nameof(Int32) && identityColumnSize != nameof(Int64))
            {
                throw new InvalidOperationException("The configured 'IdentityColumnSize' should be 'Int32' or 'Int64'.");
            }
        }

        return identityColumnSize;
    }
}
