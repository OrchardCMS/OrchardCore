using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Base class that can be used for any <see cref="IShellRemovingHostHandler"/> implementation.
/// </summary>
public class ShellRemovingHostHandler : IShellRemovingHostHandler
{
    public virtual Task RemovingAsync(ShellRemovingContext context) => Task.CompletedTask;
    public virtual Task LocalRemovingAsync(ShellRemovingContext context) => Task.CompletedTask;
}
