using System;
using System.Threading.Tasks;

namespace Orchard.Processing
{
    /// <summary>
    /// Queue an event to fire inside of an explicitly decribed shell context.
    /// </summary>
    public interface IDeferredTaskEngine
    {
        bool HasPendingTasks { get; }
        void AddTask(Func<ProcessingEngineContext, Task> task);
        Task ExecuteTasksAsync(ProcessingEngineContext context);
    }
}
