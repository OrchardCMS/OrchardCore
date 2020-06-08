using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class HasClaimFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var ret = false;
            var claimType = arguments["type"].Or(arguments.At(0)).ToStringValue();
            var claimName = arguments["name"].Or(arguments.At(1)).ToStringValue();

            if (input.ToObjectValue() is ClaimsPrincipal principal)
            {
                ret = principal.HasClaim(claimType, claimName);
            }

            return new ValueTask<FluidValue>(ret ? BooleanValue.True : BooleanValue.False);
        }
    }
}
