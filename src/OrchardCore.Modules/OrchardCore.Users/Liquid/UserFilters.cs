using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace OrchardCore.Users.Liquid
{
    public static class UserFilters
    {
        public static ValueTask<FluidValue> HasClaim(FluidValue input, FilterArguments arguments, TemplateContext context)
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

        public static ValueTask<FluidValue> UserId(FluidValue input, FilterArguments args, TemplateContext ctx)
        {
            if (input.ToObjectValue() is ClaimsPrincipal claimsPrincipal)
            {
                var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return new ValueTask<FluidValue>(NilValue.Instance);
                }

                return new ValueTask<FluidValue>(FluidValue.Create(userId, ctx.Options));
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }
    }
}
