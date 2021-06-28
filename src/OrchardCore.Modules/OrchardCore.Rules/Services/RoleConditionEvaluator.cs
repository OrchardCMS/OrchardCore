using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public class RoleConditionEvaluator : ConditionEvaluator<RoleCondition>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdentityOptions _options;
        private readonly IConditionOperatorResolver _operatorResolver;

        public RoleConditionEvaluator(
            IHttpContextAccessor httpContextAccessor, 
            IOptions<IdentityOptions> options,
            IConditionOperatorResolver operatorResolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _operatorResolver = operatorResolver;
        }

        public override ValueTask<bool> EvaluateAsync(RoleCondition condition)
        {
            var roleClaimType = _options.ClaimsIdentity.RoleClaimType;
            
            // IsInRole() & HasClaim() are case sensitive.
            var operatorComparer = _operatorResolver.GetOperatorComparer(condition.Operation);

            // Claim all if the operator is negative
            if (condition.Operation is INegateOperator)
            {
                return (_httpContextAccessor.HttpContext.User?.Claims.Where(c => c.Type == roleClaimType).All(claim => 
                    operatorComparer.Compare(condition.Operation, claim.Value, condition.Value))
                ).GetValueOrDefault() ? True : False;
            }

            return (_httpContextAccessor.HttpContext.User?.Claims.Any(claim => 
                claim.Type == roleClaimType && 
                operatorComparer.Compare(condition.Operation, claim.Value, condition.Value))
            ).GetValueOrDefault() ? True : False;
        }
    }
}
