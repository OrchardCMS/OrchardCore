using System;
using System.Threading.Tasks;

namespace Orchard.DeferredTasks
{
    /// <summary>
    /// Registers and executes tasks inside of an explicitly decribed tenant context.
    /// </summary>
    public interface IDeferredTaskEngine
    {
        bool HasPendingTasks { get; }
        void AddTask(Func<DeferredTaskContext, Task> task);
        Task ExecuteTasksAsync(DeferredTaskContext context);
    }
}
