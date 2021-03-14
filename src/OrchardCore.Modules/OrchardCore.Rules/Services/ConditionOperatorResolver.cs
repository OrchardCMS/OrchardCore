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
            if (_options.ConditionOperatorOptionByType.TryGetValue(conditionOperator.GetType(), out var option))
            {
                return option.Comparer;
            }

            throw new InvalidOperationException($"Operator comparer for '{conditionOperator.GetType().Name}; not registered");
        }
    }
}
