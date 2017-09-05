using System;
using System.Threading.Tasks;

namespace OrchardCore.DeferredTasks
{
    /// <summary>
    /// An implementation of this interface provides a way to enlist custom actions that
    /// will be executed once the request is done. Each action receives a <see cref="DeferredTaskContext"/>.
    /// Actions are executed in a new <see cref="IServiceProvider"/> scope.
    /// </summary>
    public interface IDeferredTaskEngine
    {
        bool HasPendingTasks { get; }
        void AddTask(Func<DeferredTaskContext, Task> task);
        Task ExecuteTasksAsync(DeferredTaskContext context);
    }
}
