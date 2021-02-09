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
                condition.Value = condition.Value.Substring(1);
            }

            var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

            // Tenant home page could have an empty string as a request path, where
            // the default tenant does not.
            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = "/";
            }

            var operatorResolver = _operatorResolver.GetOperatorComparer(condition.Operation);
            return new ValueTask<bool>(operatorResolver.Compare(condition.Operation, requestPath, condition.Value));
        }
    }
}