using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Manages the database tables retrieved from the migrations of a given tenant.
/// </summary>
public interface IShellDbTablesManager
{
    /// <summary>
    /// Gets the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    Task<ShellDbTablesResult> GetTablesAsync(string tenant);

    /// <summary>
    /// Removes the database tables retrieved from the migrations of the provided tenant.
    /// </summary>
    Task<ShellDbTablesResult> RemoveTablesAsync(string tenant);
}
