using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell;

public class DefaultShellReleaseManager : IShellReleaseManager
{
    private bool _release;
    private bool _deferredTaskAdded;

    public void SuspendReleaseRequest()
    {
        _release = false;
    }

    public void RequestRelease()
    {
        _release = true;

        if (_deferredTaskAdded)
        {
            return;
        }

        _deferredTaskAdded = true;

        ShellScope.AddDeferredTask(async scope =>
        {
            if (!_release)
            {
                return;
            }

            _release = false;

            var shellHost = scope.ServiceProvider.GetRequiredService<IShellHost>();
            var shellSettings = scope.ServiceProvider.GetRequiredService<ShellSettings>();

            await shellHost.ReleaseShellContextAsync(shellSettings);
        });
    }
}
