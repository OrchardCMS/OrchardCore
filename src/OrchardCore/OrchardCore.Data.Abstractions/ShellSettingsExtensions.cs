using System;
using System.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Data;

public static class ShellSettingsExtensions
{
    private const string _databaseTableSection = "OrchardCore_Data_DatabaseTable";
    private const string _defaultDocumentTableKey = $"{_databaseTableSection}:DefaultDocumentTable";
    private const string _defaultTableNameSeparatorKey = $"{_databaseTableSection}:DefaultTableNameSeparator";
    private const string _defaultIdentityColumnSizeKey = $"{_databaseTableSection}:DefaultIdentityColumnSize";

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
            shellSettings["DocumentTable"] = shellSettings.GetDocumentTable();
            shellSettings["TableNameSeparator"] = shellSettings.GetTableNameSeparator();
            shellSettings["IdentityColumnSize"] = shellSettings.GetIdentityColumnSize();
        }

        return shellSettings;
    }

    public static string GetDocumentTable(this ShellSettings shellSettings)
    {
        var documentTable = !shellSettings.IsInitialized()
            ? shellSettings[_defaultDocumentTableKey]
            : shellSettings["DocumentTable"];

        if (String.IsNullOrWhiteSpace(documentTable))
        {
            documentTable = "Document";
        }
        else
        {
            documentTable = documentTable.Trim();
        }

        return documentTable;
    }

    public static string GetTableNameSeparator(this ShellSettings shellSettings)
    {
        var tableNameSeparator = !shellSettings.IsInitialized()
            ? shellSettings[_defaultTableNameSeparatorKey]
            : shellSettings["TableNameSeparator"];

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
            ? shellSettings[_defaultIdentityColumnSizeKey]
            : shellSettings["IdentityColumnSize"];

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
