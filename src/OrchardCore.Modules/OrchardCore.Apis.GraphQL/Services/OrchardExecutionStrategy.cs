using System.Threading.Tasks;
using GraphQL.Execution;

namespace OrchardCore.Apis.GraphQL.Services
{
    // we can remove this once https://github.com/graphql-dotnet/graphql-dotnet/pull/1251 is released and just configure it.
    // basically here we're limiting paralell execution to 1 node to stop yes sql sessions getting re-used
    public class OrchardExecutionStrategy : ParallelExecutionStrategy
    {
            protected override async Task<ExecutionNode> ExecuteNodeAsync(ExecutionContext context, ExecutionNode node)
            {

            var userContext = (GraphQLContext)context.UserContext;

            await userContext.ExecutionLock.WaitAsync();

            try
            {
                return await base.ExecuteNodeAsync(context, node);
            }
            finally
            {
                userContext.ExecutionLock.Release();
            }
        }
    }
}