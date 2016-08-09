using System;
using System.Collections.Generic;

namespace Orchard.BackgroundTasks
{
    /// <summary>
    /// An implementation of <see cref="IBackgroundTaskService"/> is a registered as singleton
    /// and handles the periodic execution of background tasks.
    /// </summary>
    public interface IBackgroundTaskService
    {
        /// <summary>
        /// Activates the tasks.
        /// </summary>
        void Activate();

        /// <summary>
        /// Terminates the tasks.
        /// </summary>
        void Terminate();

        /// <summary>
        /// Returns all the tasks and their current state.
        /// </summary>
        /// <returns></returns>
        IDictionary<IBackgroundTask, BackgroundTaskState> GetTasks();

        /// <summary>
        /// Sets the period of a named task group. This has no effect until <see cref="Activate"/> is called.
        /// </summary>
        void SetDelay(string group, TimeSpan period);
    }
}
