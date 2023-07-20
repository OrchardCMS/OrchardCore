using System.Collections.Generic;

namespace OrchardCore.Data.Migration.Records
{
    /// <summary>
    /// Represents a record in the database migration.
    /// </summary>
    public class DataMigrationRecord
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DataMigrationRecord"/>.
        /// </summary>
        public DataMigrationRecord()
        {
            DataMigrations = new List<DataMigration>();
        }

        /// <summary>
        /// Gete or sets the record Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the database migrations.
        /// </summary>
        public List<DataMigration> DataMigrations { get; set; }
    }
}
