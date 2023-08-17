using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Rules.Services
{
    public class ConditionResolver : IConditionResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConditionOptions _options;

        public ConditionResolver(IServiceProvider serviceProvider, IOptions<ConditionOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public IConditionEvaluator GetConditionEvaluator(Condition condition)
        {
            if (_options.Evaluators.TryGetValue(condition.GetType(), out var conditionEvaluatorType))
            {
                return _serviceProvider.GetRequiredService(conditionEvaluatorType) as IConditionEvaluator;
            }

            throw new InvalidOperationException($"Condition evaluator for '{condition.GetType().Name}; not registered");
        }
    }
}
