using OrchardCore.Environment.Shell.Removing;

namespace OrchardCore.Modules;

public interface IModularTenantEvents
{
    Task ActivatingAsync();
    Task ActivatedAsync();
    Task TerminatingAsync();
    Task TerminatedAsync();
    Task RemovingAsync(ShellRemovingContext context);
}
