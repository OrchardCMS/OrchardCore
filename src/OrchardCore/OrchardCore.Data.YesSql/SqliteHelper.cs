using System.IO;
using Microsoft.Data.Sqlite;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class SqliteHelper
{
    private static readonly string _defaultDatabaseName = "yessql.db";

    public static string GetConnectionString(SqliteOptions sqliteOptions, ShellOptions shellOptions, string shellName) =>
        GetConnectionString(sqliteOptions, GetDatabaseFolder(shellOptions, shellName), shellName);

    public static string GetConnectionString(SqliteOptions sqliteOptions, string databaseFolder, string shellName)
    {
        var databasePath = Path.Combine(databaseFolder, shellName);
        if (!File.Exists(databaseFolder))
        {
            databasePath = Path.Combine(databaseFolder, _defaultDatabaseName);
        }

        return new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling
        }
        .ToString();
    }

    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);
}
