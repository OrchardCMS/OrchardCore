using System.Globalization;
using System.Threading.Tasks;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class CultureConditionEvaluator : ConditionEvaluator<CultureCondition>
    {
        private readonly IConditionOperatorResolver _operatorResolver;

        public CultureConditionEvaluator(IConditionOperatorResolver operatorResolver)
        {
            _operatorResolver = operatorResolver;
        }

        public override ValueTask<bool> EvaluateAsync(CultureCondition condition)
        {
            var currentCulture = CultureInfo.CurrentCulture;

            var operatorComparer = _operatorResolver.GetOperatorComparer(condition.Operation);

            var result = operatorComparer.Compare(condition.Operation, currentCulture.Name, condition.Value) ||
                operatorComparer.Compare(condition.Operation, currentCulture.Parent.Name, condition.Value);

            return result ? True : False;
        }
    }
}
