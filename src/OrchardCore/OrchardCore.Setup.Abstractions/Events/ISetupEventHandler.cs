using OrchardCore.Setup.Services;

namespace OrchardCore.Setup.Events;

/// <summary>
/// Defines methods for handling setup events, including performing setup operations and marking their success or
/// failure.
/// </summary>
/// <remarks>Implement this interface to provide custom logic for setup processes that require asynchronous
/// handling of configuration, initialization, or error reporting. Methods are designed to be called during the setup
/// lifecycle to signal progress or outcome. Implementations should ensure thread safety if setup events may be
/// triggered concurrently.
/// </remarks>
public interface ISetupEventHandler
{
    /// <summary>
    /// Performs asynchronous setup operations using the specified context.
    /// </summary>
    /// <param name="context">An object that provides configuration and state information required for the setup process. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous setup operation.</returns>
    Task SetupAsync(SetupContext context) => Task.CompletedTask;

    /// <summary>
    /// Marks the setup operation as failed within the provided context.
    /// </summary>
    /// <param name="context">The setup context in which the failure is recorded. Cannot be null.</param>
    /// <returns>A completed task that represents the asynchronous operation.</returns>
    Task FailedAsync(SetupContext context) => Task.CompletedTask;

    /// <summary>
    /// Returns a completed task that represents a successful asynchronous operation.
    /// </summary>
    /// <returns>A <see cref="Task"/> that has already completed successfully.</returns>
    Task SucceededAsync() => Task.CompletedTask;
}
