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
        private readonly string _roleClaimType;

        public IsInRoleFilter(IOptions<IdentityOptions> optionsAccessor)
        {
            _roleClaimType = optionsAccessor.Value.ClaimsIdentity.RoleClaimType;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var ret = false;

            var claimName = arguments["name"].Or(arguments.At(0)).ToStringValue();

            if (input.ToObjectValue() is ClaimsPrincipal principal)
            {
                ret = principal.HasClaim(_roleClaimType, claimName);
            }

            return new ValueTask<FluidValue>(ret ? BooleanValue.True : BooleanValue.False);
        }
    }
}
