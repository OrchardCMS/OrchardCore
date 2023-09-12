using System;
using System.Linq;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class ShellSettingsExtensions
{
    private const string DatabaseTableOptions = "OrchardCore_Data_TableOptions";
    private const string DefaultDocumentTable = $"{DatabaseTableOptions}:DefaultDocumentTable";
    private const string DefaultTableNameSeparator = $"{DatabaseTableOptions}:DefaultTableNameSeparator";
    private const string DefaultIdentityColumnSize = $"{DatabaseTableOptions}:DefaultIdentityColumnSize";

    private readonly static string[] _identityColumnSizes = new[] { nameof(Int64), nameof(Int32) };

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
        var documentTable = (!shellSettings.IsInitialized()
            ? shellSettings[DefaultDocumentTable]
            : shellSettings["DocumentTable"])
            ?.Trim();

        if (String.IsNullOrEmpty(documentTable))
        {
            documentTable = "Document";
        }

        return documentTable;
    }

    public static string GetTableNameSeparator(this ShellSettings shellSettings)
    {
        var tableNameSeparator = (!shellSettings.IsInitialized()
            ? shellSettings[DefaultTableNameSeparator]
            : shellSettings["TableNameSeparator"])
            ?.Trim();

        if (String.IsNullOrEmpty(tableNameSeparator))
        {
            tableNameSeparator = "_";
        }
        else if (tableNameSeparator == "NULL")
        {
            tableNameSeparator = String.Empty;
        }
        else if (tableNameSeparator.Any(c => c != '_'))
        {
            throw new InvalidOperationException($"The configured table name separator '{tableNameSeparator}' is invalid.");
        }

        return tableNameSeparator;
    }

    public static string GetIdentityColumnSize(this ShellSettings shellSettings)
    {
        var identityColumnSize = (!shellSettings.IsInitialized()
            ? shellSettings[DefaultIdentityColumnSize]
            : shellSettings["IdentityColumnSize"])
            ?.Trim();

        if (String.IsNullOrEmpty(identityColumnSize))
        {
            identityColumnSize = !shellSettings.IsInitialized() ? nameof(Int64) : nameof(Int32);
        }
        else if (!_identityColumnSizes.Contains(identityColumnSize))
        {
            throw new InvalidOperationException($"The configured identity column size '{identityColumnSize}' is invalid.");
        }

        return identityColumnSize;
    }
}
