using GraphQL;
using GraphQL.Execution;
using GraphQL.Language.AST;
using System;

namespace OrchardCore.Apis.GraphQL.Services
{
    /// <summary>
    /// Defines a custom execution strategy for queries such that async lambdas are not executed in parallel.
    /// c.f. https://github.com/OrchardCMS/OrchardCore/issues/3029
    /// </summary>
    public class OrchardDocumentExecuter : DocumentExecuter
    {
        private static IExecutionStrategy ParallelExecutionStrategy = new OrchardExecutionStrategy();
        private static IExecutionStrategy SerialExecutionStrategy = new SerialExecutionStrategy();
        private static IExecutionStrategy SubscriptionExecutionStrategy = new SubscriptionExecutionStrategy();

        protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
        {
            switch (context.Operation.OperationType)
            {
                case OperationType.Query:
                    return ParallelExecutionStrategy;

                case OperationType.Mutation:
                    return SerialExecutionStrategy;

                case OperationType.Subscription:
                    return SubscriptionExecutionStrategy;

                default:
                    throw new InvalidOperationException($"Unexpected OperationType {context.Operation.OperationType}");
            }
        }
    }
}
