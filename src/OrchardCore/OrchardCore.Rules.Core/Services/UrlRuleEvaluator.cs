using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class UrlRuleEvaluator : RuleEvaluator<UrlRule>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOperatorResolver _operatorResolver;

        public UrlRuleEvaluator(IHttpContextAccessor httpContextAccessor, IOperatorResolver operatorResolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _operatorResolver = operatorResolver;
        }

        public override ValueTask<bool> EvaluateAsync(UrlRule method)
        {

            if (method.Value.StartsWith("~/", StringComparison.Ordinal))
            {
                method.Value = method.Value.Substring(1);
            }

            var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

            // Tenant home page could have an empty string as a request path, where
            // the default tenant does not.
            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = "/";
            }

            // so request path is Other

            var operatorResolver = _operatorResolver.GetOperatorComparer(method.Operation);
            return new ValueTask<bool>(operatorResolver.Compare(method.Value, requestPath));
            // var t = String.Equals()


            // StringComparer.OrdinalIgnoreCase.Compare(method.Value, requestPath);

            // return url.EndsWith('*')
            //     ? requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
            //     : string.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase);





            // return new ValueTask<bool>(String.Equals("/", requestPath, StringComparison.Ordinal) || string.IsNullOrEmpty(requestPath));
        }
    }
}