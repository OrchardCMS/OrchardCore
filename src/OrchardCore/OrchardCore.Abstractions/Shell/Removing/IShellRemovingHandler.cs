using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Host service that collaborates on the removal of a given tenant.
/// </summary>
public interface IShellRemovingHandler
{
    /// <summary>
    /// Collaborates on the removal of the provided tenant.
    /// </summary>
    Task RemovingAsync(ShellRemovingContext context);
}
