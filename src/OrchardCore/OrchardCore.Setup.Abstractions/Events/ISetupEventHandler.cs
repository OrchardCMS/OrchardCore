using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Events;

/// <summary>
/// Defines methods for handling setup events.
/// </summary>
public interface ISetupEventHandler
{
    /// <summary>
    /// Occurs during the setup operation.
    /// </summary>
    /// <param name="context">An object that provides configuration and state information required for the setup process. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous setup operation.</returns>
    Task SetupAsync(SetupContext context) => Task.CompletedTask;

    /// <summary>
    /// Occurs when the setup operation failed.
    /// </summary>
    /// <param name="context">The setup context in which the failure is recorded. Cannot be null.</param>
    /// <returns>A completed task that represents the asynchronous operation.</returns>
    Task FailedAsync(SetupContext context) => Task.CompletedTask;

    /// <summary>
    /// Occurs when the setup operation succeeded.
    /// </summary>
    /// <returns>A <see cref="Task"/> that has already completed successfully.</returns>
    Task SucceededAsync() => Task.CompletedTask;
}
