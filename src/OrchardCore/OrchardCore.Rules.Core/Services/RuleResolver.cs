using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public interface IRuleResolver
    {
        IRuleEvaluator GetRuleEvaluator(Rule method);
    }

    public class RuleResolver : IRuleResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly RuleOptions _ruleOptions;

        public RuleResolver(IServiceProvider serviceProvider, IOptions<RuleOptions> ruleOptions)
        {
            _serviceProvider = serviceProvider;
            _ruleOptions = ruleOptions.Value;
        }

        public IRuleEvaluator GetRuleEvaluator(Rule method)
        {
            if (_ruleOptions.Evaluators.TryGetValue(method.GetType(), out var methodEvaluatorType))
            {
                return _serviceProvider.GetRequiredService(methodEvaluatorType) as IRuleEvaluator;
            }

            throw new InvalidOperationException($"Rule evaluator for '{method.GetType().Name}; not registered");
        }
    }
}
