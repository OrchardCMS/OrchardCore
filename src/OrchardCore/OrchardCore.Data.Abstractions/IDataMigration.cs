using YesSql.Sql;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Represents a contract for a database migration.
    /// </summary>
    public interface IDataMigration
    {
        /// <summary>
        /// Gets or sets the database schema builder.
        /// </summary>
        ISchemaBuilder SchemaBuilder { get; set; }
    }
}
