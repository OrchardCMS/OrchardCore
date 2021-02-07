using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class BooleanRuleEvaluator : RuleEvaluator<BooleanRule>
    {
        public override ValueTask<bool> EvaluateAsync(BooleanRule method)
            => new ValueTask<bool>(method.Value);
    }
}