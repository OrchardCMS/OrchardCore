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
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling
        };
        var dataSource = Path.Combine(databaseFolder, sqliteOptions.DatabaseName);
        if (!File.Exists(dataSource))
        {
            dataSource = Path.Combine(databaseFolder, SqliteOptions.OldDatabaseName);
        }

        connectionStringBuilder.DataSource = dataSource;

        return connectionStringBuilder.ToString();
    }

    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);
}
