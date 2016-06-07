using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orchard.DeferredTasks
{
    public class DeferredTaskEngine : IDeferredTaskEngine
    {
        private readonly IDeferredTaskState _processingState;
        private readonly ILogger _logger;

        public DeferredTaskEngine(IDeferredTaskState processingState, ILogger<DeferredTaskEngine> logger)
        {
            _processingState = processingState;
            _logger = logger;
        }

        public bool HasPendingTasks => _processingState.Tasks.Any();

        public void AddTask(Func<DeferredTaskContext, Task> task)
        {
            _processingState.Tasks.Add(task);
        }

        public async Task ExecuteTasksAsync(DeferredTaskContext context)
        {
            foreach(var task in _processingState.Tasks)
            {
                try
                {
                    await task(context);
                }
                catch(Exception e)
                {
                    _logger.LogError("An error occured while processing a deferred task: {0}", e);
                }
            }

            _processingState.Tasks.Clear();
        }
    }
}
