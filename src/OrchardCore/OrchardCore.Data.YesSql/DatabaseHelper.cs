using System;

namespace OrchardCore.Data;
internal class DatabaseHelper
{
    public static string GetStandardPrefix(string prefix)
    {
        if (String.IsNullOrWhiteSpace(prefix))
        {
            return String.Empty;
        }

        return prefix.Trim() + "_";
    }

    public static DatabaseProviderName GetDatabaseProviderName(string databaseProvider)
    {
        if (Enum.TryParse(databaseProvider, out DatabaseProviderName providerName))
        {
            return providerName;
        }

        return DatabaseProviderName.None;
    }
}

public enum DatabaseProviderName
{
    None,
    SqlConnection,
    Sqlite,
    MySql,
    Postgres,
}
