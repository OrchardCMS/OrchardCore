using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class IsHomepageRuleEvaluator : RuleEvaluator<IsHomepageRule>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IsHomepageRuleEvaluator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<bool> EvaluateAsync(IsHomepageRule method)
        {
            var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

            return new ValueTask<bool>(String.Equals("/", requestPath, StringComparison.Ordinal) || string.IsNullOrEmpty(requestPath));
        }
    }
}