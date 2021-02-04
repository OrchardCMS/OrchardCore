using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class UserIdFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments args, TemplateContext context)
        {
            if (input.ToObjectValue() is ClaimsPrincipal claimsPrincipal)
            {
                var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return new ValueTask<FluidValue>(NilValue.Instance);
                }

                return new ValueTask<FluidValue>(FluidValue.Create(userId));
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }
    }
}
