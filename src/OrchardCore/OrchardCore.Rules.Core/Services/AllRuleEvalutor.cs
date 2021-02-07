using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{    
    public class AllRuleEvaluator : AllRuleGroupEvaluator<AllRule>
    {      
        public AllRuleEvaluator(IRuleResolver ruleResolver) : base(ruleResolver)
        {
        }
    }
}