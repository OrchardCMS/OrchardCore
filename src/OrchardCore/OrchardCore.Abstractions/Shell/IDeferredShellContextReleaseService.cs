using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell;

public interface IDeferredShellContextReleaseService
{
    /// <summary>
    /// Adds a pending request to release the shell upon completion of the current HTTP-request.
    /// </summary>
    void RequestRelease();

    /// <summary>
    /// It suspends the pending release request to ensure that the shell remains intact when 'ProcessAsync()' is invoked.
    /// </summary>
    void SuspendReleaseRequest();

    /// <summary>
    /// It processes the release only if there's an active pending request that hasn't been suspended..
    /// </summary>
    /// <returns>Returns true if the shell was successfully released; otherwise, false.</returns>
    Task<bool> ProcessAsync();
}
