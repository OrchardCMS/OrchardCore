using System;
using Microsoft.Extensions.Options;

namespace OrchardCore.Rules.Services
{
    public class ConditionOperatorResolver : IConditionOperatorResolver
    {
        private readonly ConditionOperatorOptions _options;

        public ConditionOperatorResolver(IOptions<ConditionOperatorOptions> options)
        {
            _options = options.Value;
        }

        public IOperatorComparer GetOperatorComparer(ConditionOperator conditionOperator)
        {
            if (_options.Comparers.TryGetValue(conditionOperator.GetType(), out var operatorComparer))
            {
                return operatorComparer;
            }

            throw new InvalidOperationException($"Operator comparer for '{conditionOperator.GetType().Name}; not registered");
        }
    }
}
