using System;
using System.Linq;
using System.Text.RegularExpressions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Data;

public static class DatabaseShellSettingsExtensions
{
    public static ShellSettings ConfigureDatabaseTableOptions(this ShellSettings shellSettings)
    {
        if (!shellSettings.IsInitialized())
        {
            shellSettings["DocumentTable"] ??= shellSettings["DefaultDocumentTable"];
            shellSettings["TableNameSeparator"] ??= shellSettings["DefaultTableNameSeparator"];
            shellSettings["IdentityColumnSize"] ??= shellSettings["DefaultIdentityColumnSize"];
        }

        return shellSettings;
    }

    public static string GetDocumentTableOrDefault(this ShellSettings shellSettings)
    {
        var documentTable = shellSettings["DocumentTable"];
        if (!shellSettings.IsInitialized())
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
            if (!Regex.Match(documentTable, "^[A-Za-z_]+[A-Za-z0-9_]*$").Success)
            {
                throw new InvalidOperationException(
                    $"The 'DocumentTable' name is invalid, the configured value is '{documentTable}'.");
            }
        }

        return documentTable;
    }

    public static string GetTableNameSeparatorOrDefault(this ShellSettings shellSettings)
    {
        var tableNameSeparator = shellSettings["TableNameSeparator"];
        if (!shellSettings.IsInitialized())
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
                throw new InvalidOperationException(
                    $"The 'TableNameSeparator' should only contain underscores, the configured value is '{tableNameSeparator}'.");
            }
        }

        return tableNameSeparator;
    }

    public static string GetIdentityColumnSizeOrDefault(this ShellSettings shellSettings)
    {
        var identityColumnSize = shellSettings["IdentityColumnSize"];
        if (!shellSettings.IsInitialized())
        {
            identityColumnSize ??= shellSettings["DefaultIdentityColumnSize"];
        }

        if (String.IsNullOrWhiteSpace(identityColumnSize))
        {
            identityColumnSize = shellSettings.IsInitialized() ? nameof(Int32) : nameof(Int64);
        }
        else
        {
            identityColumnSize = identityColumnSize.Trim();
            if (identityColumnSize != nameof(Int32) && identityColumnSize != nameof(Int64))
            {
                throw new InvalidOperationException(
                    $"The 'IdentityColumnSize' should be 'Int32' or 'Int64', the configured value is '{identityColumnSize}'.");
            }
        }

        return identityColumnSize;
    }

    public static bool IsInitialized(this ShellSettings shellSettings) =>
        shellSettings.State != TenantState.Uninitialized && shellSettings.State != TenantState.Initializing;
}
