using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Host service managing the removal of a given tenant.
/// </summary>
public interface IShellRemovalManager
{
    /// <summary>
    /// Removes the provided tenant.
    /// </summary>
    /// <param name="shellSettings">The tenant's <see cref="ShellSettings"/>.</param>
    /// <param name="localResourcesOnly">
    /// Indicates that only local (not shared among server nodes in case of a multi-node hosting evironment) resources should be removed.
    /// Used to sync locally a tenant that was removed by another instance.
    /// </param>
    /// <returns>A <see cref="ShellRemovingContext"/>.</returns>
    Task<ShellRemovingContext> RemoveAsync(ShellSettings shellSettings, bool localResourcesOnly = false);
}
