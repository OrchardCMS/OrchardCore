using System.IO;
using Microsoft.Data.Sqlite;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class SqliteHelper
{
    public static string GetConnectionString(SqliteOptions sqliteOptions, ShellOptions shellOptions, string shellName) =>
        GetConnectionString(sqliteOptions, GetDatabaseFolder(shellOptions, shellName));

    public static string GetConnectionString(SqliteOptions sqliteOptions, string databaseFolder)
    {
        // For backward compatibility, we assume that the database name is 'yessql.db'.
        // If shellSettings["DatabaseName"] has a value, we use the name provided in the shell settings.
        if (string.IsNullOrEmpty(sqliteOptions.DatabaseName))
        {
            sqliteOptions.SetDatabaseName("yessql.db");
        }

        return new SqliteConnectionStringBuilder
        {
            DataSource = string.IsNullOrEmpty(databaseFolder)
            ? sqliteOptions.DatabaseName
            : Path.Combine(databaseFolder, sqliteOptions.DatabaseName),
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling
        }
        .ToString();
    }
    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);
}
