using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Host service that collaborates to the removing of a given tenant.
/// </summary>
public interface IShellRemovingHostHandler
{
    /// <summary>
    /// Collaborates to the removing of the provided tenant.
    /// </summary>
    Task RemovingAsync(ShellRemovingContext context);

    /// <summary>
    /// In a distributed environment, after a tenant has been removed by a given instance,
    /// this method gets called for further local removing on all other running instances.
    /// </summary>
    Task LocalRemovingAsync(ShellRemovingContext context);
}
