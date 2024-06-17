namespace OrchardCore.Environment.Shell;

public interface IShellReleaseManager
{
    /// <summary>
    /// Adds a pending request to release the shell upon completion of the current HTTP-request.
    /// </summary>
    void RequestRelease();

    /// <summary>
    /// It suspends the pending release request to ensure that the shell remains intact when 'ProcessAsync()' is invoked.
    /// </summary>
    void SuspendReleaseRequest();
}
