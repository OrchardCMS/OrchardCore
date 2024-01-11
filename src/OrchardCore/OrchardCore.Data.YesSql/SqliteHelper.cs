using System.IO;
using Microsoft.Data.Sqlite;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class SqliteHelper
{
    public static string GetConnectionString(SqliteOptions sqliteOptions, ShellOptions shellOptions, string shellName) =>
        GetConnectionString(sqliteOptions, GetDatabaseFolder(shellOptions, shellName));

    public static string GetConnectionString(SqliteOptions sqliteOptions, string databaseFolder) =>
        new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(databaseFolder, "yessql.db"),
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling
        }
        .ToString();

    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);
}
