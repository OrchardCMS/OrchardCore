using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Options;

/// <summary>
/// Queues an options invalidation request so <see cref="IOptionsMonitor{TOptions}"/>
/// instances that are wired to Orchard Core's signal-backed change tokens are refreshed
/// after the current shell scope commits successfully.
/// </summary>
public interface IOptionsUpdateNotifier
{
    /// <summary>
    /// Queues an update notification for the specified options instance.
    /// The options type must have a corresponding <see cref="IOptionsChangeTokenSource{TOptions}"/>
    /// registration that listens to Orchard Core's update signal for the invalidation to be observed.
    /// </summary>
    /// <typeparam name="TOptions">The options type to invalidate.</typeparam>
    /// <param name="name">The named options instance to invalidate.</param>
    IOptionsUpdateNotifier RequestUpdate<TOptions>(string name = "") where TOptions : class;
}
