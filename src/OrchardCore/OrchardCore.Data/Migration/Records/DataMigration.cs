namespace OrchardCore.Data.Migration.Records
{
    /// <summary>
    /// Represents a database migration.
    /// </summary>
    public class DataMigration
    {
        /// <summary>
        /// Gets or sets a class for the database migration.
        /// </summary>
        public string DataMigrationClass { get; set; }

        /// <summary>
        /// Gets or sets the version of the database migration.
        /// </summary>
        public int? Version { get; set; }
    }
}
