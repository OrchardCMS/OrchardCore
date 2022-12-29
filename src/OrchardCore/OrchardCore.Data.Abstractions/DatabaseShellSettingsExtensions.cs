using System;
using System.Linq;
using System.Text.RegularExpressions;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Data;

public static class DatabaseShellSettingsExtensions
{
    public static DatabaseTableOptions GetDatabaseTableOptions(this ShellSettings shellSettings) =>
        new()
        {
            DocumentTable = shellSettings.GetDocumentTable(),
            TableNameSeparator = shellSettings.GetTableNameSeparator(),
            IdentityColumnSize = shellSettings.GetIdentityColumnSize(),
        };

    public static ShellSettings ConfigureDatabaseTableOptions(this ShellSettings shellSettings)
    {
        if (!shellSettings.IsInitialized())
        {
            shellSettings["internal_DocumentTable"] = shellSettings.GetDocumentTable();
            shellSettings["internal_TableNameSeparator"] = shellSettings.GetTableNameSeparator();
            shellSettings["internal_IdentityColumnSize"] = shellSettings.GetIdentityColumnSize();
        }

        return shellSettings;
    }

    public static string GetDocumentTable(this ShellSettings shellSettings)
    {
        var documentTable = !shellSettings.IsInitialized()
            ? shellSettings["OrchardCore_Data_DatabaseTable:DefaultDocumentTable"]
            : shellSettings["internal_DocumentTable"];

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

    public static string GetTableNameSeparator(this ShellSettings shellSettings)
    {
        var tableNameSeparator = !shellSettings.IsInitialized()
            ? shellSettings["OrchardCore_Data_DatabaseTable:DefaultTableNameSeparator"]
            : shellSettings["internal_TableNameSeparator"];

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

    public static string GetIdentityColumnSize(this ShellSettings shellSettings)
    {
        var identityColumnSize = !shellSettings.IsInitialized()
            ? shellSettings["OrchardCore_Data_DatabaseTable:DefaultIdentityColumnSize"]
            : shellSettings["internal_IdentityColumnSize"];

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
