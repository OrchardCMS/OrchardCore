using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class RuleContainerEvaluator : AllRuleGroupEvaluator<RuleContainer>
    {      
        public RuleContainerEvaluator(IRuleResolver ruleResolver) : base(ruleResolver)
        {
        }
    }
}