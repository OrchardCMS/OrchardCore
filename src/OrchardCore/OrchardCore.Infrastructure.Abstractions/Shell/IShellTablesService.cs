using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Allows to retrieve and remove all the migrated tables of a given tenant.
/// </summary>
public interface IShellTablesService
{
    /// <summary>
    /// Retrieves all the migrated tables of the provided tenant.
    /// </summary>
    Task<ShellTablesResult> GetTablesAsync(string tenant);

    /// <summary>
    /// Removes all the migrated tables of the provided tenant.
    /// </summary>
    Task<ShellTablesResult> RemoveTablesAsync(string tenant);
}
