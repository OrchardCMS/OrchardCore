using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Shell;

public class DefaultShellReleaseManager : IShellReleaseManager
{
    public void SuspendReleaseRequest()
    {
        if (ShellScope.TryGetValue<ShellReleaseRequestContext>(ShellReleaseRequestContext.ShellScopeKey, out var context))
        {
            context.Release = false;

            ShellScope.Set(ShellReleaseRequestContext.ShellScopeKey, context);
        }
    }

    public void RequestRelease()
    {
        ShellScope.Set(ShellReleaseRequestContext.ShellScopeKey,
            new ShellReleaseRequestContext()
            {
                Release = true
            });
    }
}
