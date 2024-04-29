using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell;

public class DeferredShellContextReleaseService : IDeferredShellContextReleaseService
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    private bool _release;

    public DeferredShellContextReleaseService(
        IShellHost shellHost,
        ShellSettings shellSettings)
    {
        _shellHost = shellHost;
        _shellSettings = shellSettings;
    }

    public void SuspendReleaseRequest()
    {
        _release = false;
    }

    public void RequestRelease()
    {
        _release = true;
    }

    public async Task<bool> ProcessAsync()
    {
        if (!_release)
        {
            return false;
        }

        _release = false;

        await _shellHost.ReleaseShellContextAsync(_shellSettings);

        return true;
    }
}
