using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrchardCore.DeferredTasks
{
    public class DeferredTaskEngine : IDeferredTaskEngine
    {
        private readonly IDeferredTaskState _deferredTaskState;
        private readonly ILogger _logger;

        public DeferredTaskEngine(IDeferredTaskState deferredTaskState, ILogger<DeferredTaskEngine> logger)
        {
            _deferredTaskState = deferredTaskState;
            _logger = logger;
        }

        public bool HasPendingTasks => _deferredTaskState.Tasks.Any();

        public void AddTask(Func<DeferredTaskContext, Task> task, int order = 0)
        {
            _deferredTaskState.Tasks.Add(new DeferredTask { Task = task, Order = order });
        }

        public async Task ExecuteTasksAsync(DeferredTaskContext context)
        {
            var deferredTasks = _deferredTaskState.Tasks.OrderBy(t => t.Order).ToArray();

            for (var i = 0; i < deferredTasks.Length; i++)
            {
                var deferredTask = deferredTasks[i];

                try
                {
                    await deferredTask.Task(context);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occured while processing a deferred task");
                }
            }

            _deferredTaskState.Tasks.Clear();
        }
    }
}
