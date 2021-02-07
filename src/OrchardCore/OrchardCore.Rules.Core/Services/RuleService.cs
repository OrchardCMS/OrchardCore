using System;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public interface IRuleService
    {
        ValueTask<bool> EvaluateAsync(AllRule methodGroup);
    }

    public class RuleService : IRuleService
    {
        private readonly IRuleResolver _ruleResolver;

        public RuleService(IRuleResolver ruleResolver)
        {
            _ruleResolver = ruleResolver;
        }

        public ValueTask<bool> EvaluateAsync(AllRule methodGroup)
            => _ruleResolver.GetRuleEvaluator(methodGroup).EvaluateAsync(methodGroup);
    }
}