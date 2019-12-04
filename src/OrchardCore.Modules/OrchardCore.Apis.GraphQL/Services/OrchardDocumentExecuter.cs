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
        private static readonly IExecutionStrategy _parallelExecutionStrategy = new OrchardExecutionStrategy();
        private static readonly IExecutionStrategy _serialExecutionStrategy = new SerialExecutionStrategy();
        private static readonly IExecutionStrategy _subscriptionExecutionStrategy = new SubscriptionExecutionStrategy();

        protected override IExecutionStrategy SelectExecutionStrategy(ExecutionContext context)
        {
            return context.Operation.OperationType switch
            {
                OperationType.Query => _parallelExecutionStrategy,
                OperationType.Mutation => _serialExecutionStrategy,
                OperationType.Subscription => _subscriptionExecutionStrategy,
                _ => throw new InvalidOperationException($"Unexpected OperationType {context.Operation.OperationType}"),
            };
        }
    }
}
