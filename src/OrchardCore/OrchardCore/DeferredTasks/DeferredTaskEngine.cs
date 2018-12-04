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

        public void AddTask(Func<DeferredTaskContext, Task> task)
        {
            _deferredTaskState.Tasks.Add(task);
        }

        public async Task ExecuteTasksAsync(DeferredTaskContext context)
        {
            for (var i = 0; i < _deferredTaskState.Tasks.Count; i++)
            {
                var task = _deferredTaskState.Tasks[i];

                try
                {
                    await task(context);
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
