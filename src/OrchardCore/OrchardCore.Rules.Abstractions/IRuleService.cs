using System.Threading.Tasks;

namespace OrchardCore.Rules
{
    public interface IRuleService
    {
        ValueTask<bool> EvaluateAsync(Rule rule);
    }
}
