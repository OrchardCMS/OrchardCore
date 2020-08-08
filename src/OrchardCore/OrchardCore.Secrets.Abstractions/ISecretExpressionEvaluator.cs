using System.Threading.Tasks;

namespace OrchardCore.Secrets
{
    public interface ISecretExpressionEvaluator
    {
        Task<string> EvaluateAsync(string template);
        bool IsSecretExpression(string template);
    }
}
