using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Execution;

namespace OrchardCore.Apis.GraphQL.Services
{
    // we can remove this once https://github.com/graphql-dotnet/graphql-dotnet/pull/1251 is released and just configure it.
    // basically here we're limiting paralell execution to 1 node to stop yes sql sessions getting re-used
    public class OrchardExecutionStrategy : ParallelExecutionStrategy
    {
        const int _maxParallelExecutionCount = 1;

        protected override Task ExecuteNodeTreeAsync(ExecutionContext context, ObjectExecutionNode rootNode)
            => ExecuteNodeTreeAsync(context, rootNode);

        protected async Task ExecuteNodeTreeAsync(ExecutionContext context, ExecutionNode rootNode)
        {
            var pendingNodes = new Queue<ExecutionNode>();
            pendingNodes.Enqueue(rootNode);

            var currentTasks = new List<Task<ExecutionNode>>();
            while (pendingNodes.Count > 0)
            {
                // Start executing pending nodes, while limiting the maximum number of parallel executed nodes to the set limit
                while (currentTasks.Count <  _maxParallelExecutionCount && pendingNodes.Count > 0)
                {
                    context.CancellationToken.ThrowIfCancellationRequested();
                    var pendingNode = pendingNodes.Dequeue();
                    var pendingNodeTask = ExecuteNodeAsync(context, pendingNode);
                    if (pendingNodeTask.IsCompleted)
                    {
                        // Node completed synchronously, so no need to add it to the list of currently executing nodes
                        // instead add any child nodes to the pendingNodes queue directly here
                        var result = await pendingNodeTask;
                        if (result is IParentExecutionNode parentExecutionNode)
                        {
                            foreach (var childNode in parentExecutionNode.GetChildNodes())
                            {
                                pendingNodes.Enqueue(childNode);
                            }
                        }
                    }
                    else
                    {
                        // Node is actually asynchronous, so add it to the list of current tasks being executed in parallel
                        currentTasks.Add(pendingNodeTask);
                    }

                }

                await OnBeforeExecutionStepAwaitedAsync(context)
                    .ConfigureAwait(false);

                // Await tasks for this execution step
                var completedNodes = await Task.WhenAll(currentTasks)
                    .ConfigureAwait(false);
                currentTasks.Clear();

                // Add child nodes to pending nodes to execute the next level in parallel
                foreach (var childNode in completedNodes
                    .OfType<IParentExecutionNode>()
                    .SelectMany(x => x.GetChildNodes()))
                {
                    pendingNodes.Enqueue(childNode);
                }
            }
        }
    }
}