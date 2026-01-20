using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public static class UserFilters
    {
        public static ValueTask<FluidValue> HasClaim(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (input.ToObjectValue() is LiquidUserAccessor)
            {
                var httpContextAccessor = ((LiquidTemplateContext)ctx).Services.GetRequiredService<IHttpContextAccessor>();

                var user = httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    var claimType = arguments["type"].Or(arguments.At(0)).ToStringValue();
                    var claimName = arguments["name"].Or(arguments.At(1)).ToStringValue();

                    if (user.HasClaim(claimType, claimName))
                    {
                        return new ValueTask<FluidValue>(BooleanValue.True);
                    }
                }
            }

            return new ValueTask<FluidValue>(BooleanValue.False);
        }

        public static ValueTask<FluidValue> UserId(FluidValue input, FilterArguments _, TemplateContext ctx)
        {
            if (input.ToObjectValue() is LiquidUserAccessor)
            {
                var httpContextAccessor = ((LiquidTemplateContext)ctx).Services.GetRequiredService<IHttpContextAccessor>();

                var user = httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != null)
                    {
                        return new ValueTask<FluidValue>(FluidValue.Create(userId, ctx.Options));
                    }
                }
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }
    }
}
