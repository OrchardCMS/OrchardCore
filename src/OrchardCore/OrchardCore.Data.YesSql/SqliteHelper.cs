using System.IO;
using Microsoft.Data.Sqlite;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class SqliteHelper
{
    public static string GetConnectionString(SqliteOptions sqliteOptions, ShellOptions shellOptions, string shellName, string databaseName = "", bool createFile = false)
        => GetConnectionString(sqliteOptions, GetDatabaseFolder(shellOptions, shellName), databaseName, createFile);

    public static string GetConnectionString(SqliteOptions sqliteOptions, string databaseFolder,  string databaseName = "", bool createFile = false)
    {
        if (string.IsNullOrEmpty(databaseName))
        {
            // For backward compatibility, we assume that the database name is 'yessql.db'.
            databaseName = "yessql.db";
        }

        return new SqliteConnectionStringBuilder
        {
            DataSource = string.IsNullOrEmpty(databaseFolder)
            ? databaseName
            : Path.Combine(databaseFolder, databaseName),
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling,
            Mode = createFile ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite
        }
        .ToString();
    }

    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);
}
