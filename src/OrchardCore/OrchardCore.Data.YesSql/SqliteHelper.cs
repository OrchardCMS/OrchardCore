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

        // Only allow creating a file DB when a tenant is in the Initializing state.
        var sqliteOpenMode = shellSettings.IsInitializing() ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite;

        return GetSqliteConnectionStringBuilder(sqliteOptions, databaseFolder, databaseName, sqliteOpenMode).ToString();
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
        ArgumentException.ThrowIfNullOrEmpty(databaseFolder);
        ArgumentException.ThrowIfNullOrEmpty(databaseName);

        return GetSqliteConnectionStringBuilder(sqliteOptions, databaseFolder, databaseName, sqliteOpenMode).ToString();
    }

    public static string GetDatabaseFolder(ShellOptions shellOptions, string shellName) =>
        Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellName);

    private static SqliteConnectionStringBuilder GetSqliteConnectionStringBuilder(SqliteOptions sqliteOptions, string databaseFolder, string databaseName, SqliteOpenMode sqliteOpenMode)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = string.IsNullOrEmpty(databaseFolder) ? databaseName : Path.Combine(databaseFolder, databaseName),
            Cache = SqliteCacheMode.Shared,
            Pooling = sqliteOptions.UseConnectionPooling,
            Mode = sqliteOpenMode
        };
    }
}
