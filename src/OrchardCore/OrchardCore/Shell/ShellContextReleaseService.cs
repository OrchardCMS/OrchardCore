using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell;

public class ShellContextReleaseService : IShellContextReleaseService
{
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private bool _release { get; set; }

    public ShellContextReleaseService(
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
        if (_release)
        {
            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            _release = false;

            return true;
        }

        return false;
    }
}
