using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class UrlConditionEvaluator : ConditionEvaluator<UrlCondition>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConditionOperatorResolver _operatorResolver;

        public UrlConditionEvaluator(IHttpContextAccessor httpContextAccessor, IConditionOperatorResolver operatorResolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _operatorResolver = operatorResolver;
        }

        public override ValueTask<bool> EvaluateAsync(UrlCondition condition)
        {
            if (condition.Value.StartsWith("~/", StringComparison.Ordinal))
            {
                condition.Value = condition.Value[1..];
            }

            var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

            // Tenant home page could have an empty string as a request path, where
            // the default tenant does not.
            if (String.IsNullOrEmpty(requestPath))
            {
                requestPath = "/";
            }

            var operatorComparer = _operatorResolver.GetOperatorComparer(condition.Operation);
            return operatorComparer.Compare(condition.Operation, requestPath, condition.Value) ? True : False;
        }
    }
}
