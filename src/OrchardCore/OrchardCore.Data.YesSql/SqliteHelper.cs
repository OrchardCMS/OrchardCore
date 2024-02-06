using System;
using System.IO;
using Microsoft.Data.Sqlite;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public static class SqliteHelper
{
    public static string GetConnectionString(SqliteOptions sqliteOptions, ShellOptions shellOptions, ShellSettings shellSettings)
        => GetConnectionString(sqliteOptions, GetDatabaseFolder(shellOptions, shellSettings.Name), shellSettings);

    /// <summary>
    /// Orchard Core SQLite connection string helper that requires ShellSettings to be passed
    /// to define the databaseName and the SQLite mode.
    /// </summary>
    /// <param name="sqliteOptions"></param>
    /// <param name="databaseFolder"></param>
    /// <param name="shellSettings"></param>
    /// <returns></returns>
    public static string GetConnectionString(SqliteOptions sqliteOptions, string databaseFolder, ShellSettings shellSettings)
    {
        var databaseName = shellSettings["DatabaseName"];

        if (string.IsNullOrEmpty(databaseName))
        {
            // For backward compatibility, we assume that the database name is 'yessql.db'.
            databaseName = "yessql.db";
        }

        return new SqliteConnectionStringBuilder
        {
            DataSource = string.IsNullOrEmpty(databaseFolder) ? databaseName : Path.Combine(databaseFolder, databaseName),
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling,
            Mode = shellSettings.IsInitializing() ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite
        }
        .ToString();
    }

    /// <summary>
    /// Generic helper for any other SQLite connection.
    /// </summary>
    /// <param name="sqliteOptions"></param>
    /// <param name="databaseFolder"></param>
    /// <param name="databaseName"></param>
    /// <param name="sqliteOpenMode"></param>
    /// <returns></returns>
    public static string GetConnectionString(SqliteOptions sqliteOptions, string databaseFolder, string databaseName, SqliteOpenMode sqliteOpenMode)
    {
        ArgumentException.ThrowIfNullOrEmpty(databaseFolder, nameof(databaseFolder));
        ArgumentException.ThrowIfNullOrEmpty(databaseName, nameof(databaseName));

        return new SqliteConnectionStringBuilder
        {
            DataSource = string.IsNullOrEmpty(databaseFolder) ? databaseName : Path.Combine(databaseFolder, databaseName),
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling,
            Mode = sqliteOpenMode
        }
        .ToString();
    }

    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);
}
