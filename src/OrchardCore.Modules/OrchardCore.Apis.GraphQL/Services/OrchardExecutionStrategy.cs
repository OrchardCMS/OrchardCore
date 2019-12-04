using System.Threading.Tasks;
using GraphQL.Execution;

namespace OrchardCore.Apis.GraphQL.Services
{
    // limit execution to prevent yessql session issues
    public class OrchardExecutionStrategy : ParallelExecutionStrategy
    {
        protected override Task<ExecutionNode> ExecuteNodeAsync(ExecutionContext context, ExecutionNode node)
        {
            lock (context.UserContext)
            {
                return base.ExecuteNodeAsync(context, node);
            }
        }
    }
}
