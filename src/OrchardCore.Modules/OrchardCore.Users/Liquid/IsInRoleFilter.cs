using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class IsInRoleFilter : ILiquidFilter
    {
        private readonly IdentityOptions _identityOptions;

        public IsInRoleFilter(IOptions<IdentityOptions> identityOptions)
        {
            _identityOptions = identityOptions.Value;
        }
        
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var ret = false;

            var claimName = arguments["name"].Or(arguments.At(0)).ToStringValue();

            var roleClaimType = _identityOptions.ClaimsIdentity.RoleClaimType;

            if (input.ToObjectValue() is ClaimsPrincipal principal)
            {
                ret = principal.Claims.Any(claim => claim.Type == roleClaimType && claim.Value.Equals(claimName, StringComparison.OrdinalIgnoreCase));
            }

            return new ValueTask<FluidValue>(ret ? BooleanValue.True : BooleanValue.False);
        }
    }
}
