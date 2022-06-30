using System.Threading.Tasks;

namespace OrchardCore.Data.Migration
{
    /// <summary>
    /// Allows to explores the tables from tenant data migrations.
    /// </summary>
    public interface IDataMigrationExplorer
    {
        /// <summary>
        /// Retrieves the tables from the data migrations of this tenant.
        /// </summary>
        Task<SchemaExplorerResult> GetTablesAsync(string tenant);

        /// <summary>
        /// Removes the tables retrieved from the data migrations of this tenant.
        /// </summary>
        Task<SchemaExplorerResult> RemoveTablesAsync(string tenant);
    }
}
