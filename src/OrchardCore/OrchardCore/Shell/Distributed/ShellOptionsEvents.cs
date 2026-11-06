using OrchardCore.Environment.Shell.Events;

namespace OrchardCore.Environment.Shell.Distributed;

/// <summary>
/// Host level events raised when the options of a tenant have been updated, used to keep in sync
/// the options of the same tenant on other instances.
/// </summary>
internal sealed class ShellOptionsEvents
{
    /// <summary>
    /// The <see cref="ShellEvent"/> delegate that will get called after the options of a given shell have been updated.
    /// </summary>
    public ShellEvent UpdatedAsync { get; set; }

    /// <summary>
    /// Invokes the <see cref="UpdatedAsync"/> delegates for the specified tenant.
    /// </summary>
    public async Task InvokeUpdatedAsync(string name)
    {
        if (UpdatedAsync is null)
        {
            return;
        }

        foreach (var handler in UpdatedAsync.GetInvocationList())
        {
            await ((ShellEvent)handler)(name);
        }
    }
}
