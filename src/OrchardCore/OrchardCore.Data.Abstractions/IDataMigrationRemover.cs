using System.Threading.Tasks;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Analyzes database migrations.
    /// Allows to remove the database tables of a given tenant.
    /// </summary>
    public interface IDataMigrationRemover
    {
        /// <summary>
        /// Gets the table names related to the provided tenant name.
        /// </summary>
        Task<RemoveSchemaResult> GetTablesOnlyAsync(string tenantName);

        /// <summary>
        /// Removes the tables related to the provided tenant name.
        /// </summary>
        Task<RemoveSchemaResult> RemoveTablesAsync(string tenantName);
    }
}
