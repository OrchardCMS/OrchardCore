
using System.Threading.Tasks;
using GraphQL.Execution;

namespace OrchardCore.Apis.GraphQL.Services
{
    // limit execution to prevent yessql session issues
    public class OrchardExecutionStrategy : ParallelExecutionStrategy
    {
        System.Threading.SemaphoreSlim x = new System.Threading.SemaphoreSlim(1, 1);
        object foo = new object();

        protected override Task<ExecutionNode> ExecuteNodeAsync(ExecutionContext context, ExecutionNode node)
        {
           lock (foo)
            {
                return base.ExecuteNodeAsync(context, node);
            }
        }
    }
}
