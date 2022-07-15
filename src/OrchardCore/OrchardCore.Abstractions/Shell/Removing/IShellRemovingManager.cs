using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Host service managing the removing of a given tenant.
/// </summary>
public interface IShellRemovingManager
{
    /// <summary>
    /// Removes the provided tenant.
    /// </summary>
    /// <param name="shellSettings">The tenant <see cref="ShellSettings"/>.</param>
    /// <param name="localResourcesOnly">
    /// Indicates that only local (non shared) resources should be removed.
    /// Used to sync locally a tenant that was removed by another instance.
    /// </param>
    /// <returns>A <see cref="ShellRemovingContext"/>.</returns>
    Task<ShellRemovingContext> RemoveAsync(ShellSettings shellSettings, bool localResourcesOnly = false);
}
