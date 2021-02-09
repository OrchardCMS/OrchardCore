using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class HomepageConditionEvaluator : ConditionEvaluator<HomepageCondition>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomepageConditionEvaluator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<bool> EvaluateAsync(HomepageCondition condition)
        {
            var requestPath = _httpContextAccessor.HttpContext.Request.Path.Value;

            if (condition.Value)
            {
                return new ValueTask<bool>(String.Equals("/", requestPath, StringComparison.Ordinal) || String.IsNullOrEmpty(requestPath));
            }
            else
            {
                return new ValueTask<bool>(!String.Equals("/", requestPath, StringComparison.Ordinal) && !String.IsNullOrEmpty(requestPath));
            }
        }
    }
}