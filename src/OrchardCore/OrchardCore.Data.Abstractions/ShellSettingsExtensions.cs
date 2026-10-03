using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class ShellSettingsExtensions
{
    private const string DatabaseTableOptions = "Data:TableOptions";
    private const string DefaultDocumentTable = $"{DatabaseTableOptions}:DefaultDocumentTable";
    private const string DefaultTableNameSeparator = $"{DatabaseTableOptions}:DefaultTableNameSeparator";
    private const string DefaultIdentityColumnSize = $"{DatabaseTableOptions}:DefaultIdentityColumnSize";

    // The 'OrchardCore_Data_TableOptions' section is deprecated and will be removed in a future major version, use 'Data:TableOptions' instead.
    private const string LegacyDefaultDocumentTable = "OrchardCore_Data_TableOptions:DefaultDocumentTable";
    private const string LegacyDefaultTableNameSeparator = "OrchardCore_Data_TableOptions:DefaultTableNameSeparator";
    private const string LegacyDefaultIdentityColumnSize = "OrchardCore_Data_TableOptions:DefaultIdentityColumnSize";

    private static readonly string[] s_identityColumnSizes = { nameof(Int64), nameof(Int32) };

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
            ? shellSettings[DefaultDocumentTable] ?? shellSettings[LegacyDefaultDocumentTable]
            : shellSettings["DocumentTable"])
            ?.Trim();

        if (string.IsNullOrEmpty(documentTable))
        {
            documentTable = "Document";
        }

        return documentTable;
    }

    public static string GetTableNameSeparator(this ShellSettings shellSettings)
    {
        var tableNameSeparator = (!shellSettings.IsInitialized()
            ? shellSettings[DefaultTableNameSeparator] ?? shellSettings[LegacyDefaultTableNameSeparator]
            : shellSettings["TableNameSeparator"])
            ?.Trim();

        if (string.IsNullOrEmpty(tableNameSeparator))
        {
            tableNameSeparator = "_";
        }
        else if (tableNameSeparator == "NULL")
        {
            tableNameSeparator = string.Empty;
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
            ? shellSettings[DefaultIdentityColumnSize] ?? shellSettings[LegacyDefaultIdentityColumnSize]
            : shellSettings["IdentityColumnSize"])
            ?.Trim();

        if (string.IsNullOrEmpty(identityColumnSize))
        {
            identityColumnSize = !shellSettings.IsInitialized() ? nameof(Int64) : nameof(Int32);
        }
        else if (!s_identityColumnSizes.Contains(identityColumnSize))
        {
            throw new InvalidOperationException($"The configured identity column size '{identityColumnSize}' is invalid.");
        }

        return identityColumnSize;
    }
}
