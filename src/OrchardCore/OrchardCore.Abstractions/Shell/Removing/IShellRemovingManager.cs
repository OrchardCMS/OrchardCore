using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Host service managing the removing process of a given tenant.
/// </summary>
public interface IShellRemovingManager
{
    /// <summary>
    /// Removes the provided tenant.
    /// </summary>
    Task<ShellRemovingContext> RemoveAsync(string tenant);
}
