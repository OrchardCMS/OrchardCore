namespace OrchardCore.Data
{
    /// <summary>
    /// Sqlite-specific configuration for the Orchard Core database. 
    /// See <see href="https://docs.orchardcore.net/en/latest/docs/reference/core/Data/#sqlite" />
    /// </summary>
    public class SqliteOptions
    {
        /// <summary>
        /// <para>By default in .Net 6, <c>Microsoft.Data.Sqlite</c> pools connections to the database. 
        /// It achieves this by putting a lock on the database file and leaving connections open to be reused.
        /// If the lock is preventing tasks like backups, this functionality can be disabled.</para>
        /// 
        /// <para>There may be a performance penalty associated with disabling connection pooling.</para>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/connection-strings#pooling" />
        /// </summary>
        public bool UseConnectionPooling { get; set; }
    }
}
