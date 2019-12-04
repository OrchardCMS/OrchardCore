//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using GraphQL.Execution;

//namespace OrchardCore.Apis.GraphQL.Services
//{
//    // we can remove this once https://github.com/graphql-dotnet/graphql-dotnet/pull/1251 is released and just configure it.
//    // basically here we're limiting paralell execution to 1 node to stop yes sql sessions getting re-used
//    public class OrchardExecutionStrategy : ParallelExecutionStrategy
//    {
//        protected override Task ExecuteNodeTreeAsync(ExecutionContext context, ObjectExecutionNode rootNode)
//        => ExecuteNodeTreeAsync(context, rootNode);

//        protected async Task ExecuteNodeTreeAsync(ExecutionContext context, ExecutionNode rootNode)
//        {
//            var pendingNodes = new List<ExecutionNode>
//            {
//                rootNode
//            };

//            object f = new object();

//            while (pendingNodes.Count > 0)
//            {
//                var currentTasks = new Task<ExecutionNode>[pendingNodes.Count];

//                // Start executing all pending nodes
//                for (int i = 0; i < pendingNodes.Count; i++)
//                {
//                    context.CancellationToken.ThrowIfCancellationRequested();
//                    currentTasks[i] = ExecuteNodeAsync(context, pendingNodes[i], f);
//                }

//                pendingNodes.Clear();

//                await OnBeforeExecutionStepAwaitedAsync(context)
//                    .ConfigureAwait(false);

//                // Await tasks for this execution step
//                var completedNodes = await Task.WhenAll(currentTasks)
//                    .ConfigureAwait(false);

//                // Add child nodes to pending nodes to execute the next level in parallel
//                var childNodes = completedNodes
//                    .OfType<IParentExecutionNode>()
//                    .SelectMany(x => x.GetChildNodes());

//                pendingNodes.AddRange(childNodes);
//            }
//        }

//        protected Task<ExecutionNode> ExecuteNodeAsync(ExecutionContext context, ExecutionNode node, object d)
//            {

//            var userContext = (GraphQLContext)context.UserContext;

//            lock (d)
//            {
//                return base.ExecuteNodeAsync(context, node);
//            }
//        }
//    }
//}