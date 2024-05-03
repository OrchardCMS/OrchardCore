using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell;

public class DefaultShellReleaseManager : IShellReleaseManager
{
    public void SuspendReleaseRequest()
    {
        var context = ShellScope.Get<ShellReleaseRequestContext>(nameof(ShellReleaseRequestContext));

        if (context is not null)
        {
            context.Release = false;

            ShellScope.Set(nameof(ShellReleaseRequestContext), context);
        }
    }

    public void RequestRelease()
    {
        ShellScope.Set(nameof(ShellReleaseRequestContext),
            new ShellReleaseRequestContext()
            {
                Release = true
            });
    }
}
