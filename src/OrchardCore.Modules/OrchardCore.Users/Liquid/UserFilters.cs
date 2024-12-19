using System.Security.Claims;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid;

public static class UserFilters
{
    public static ValueTask<FluidValue> HasClaim(FluidValue input, FilterArguments arguments, TemplateContext ctx)
    {
        if (input.ToObjectValue() is LiquidUserAccessor)
        {
            var context = (LiquidTemplateContext)ctx;
            var httpContextAccessor = context.Services.GetRequiredService<IHttpContextAccessor>();

            var user = httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                var claimType = arguments["type"].Or(arguments.At(0)).ToStringValue();
                var claimName = arguments["name"].Or(arguments.At(1)).ToStringValue();

                if (user.HasClaim(claimType, claimName))
                {
                    return BooleanValue.True;
                }
            }
        }

        return BooleanValue.False;
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
                    return ValueTask.FromResult(FluidValue.Create(userId, ctx.Options));
                }
            }
        }

        return ValueTask.FromResult<FluidValue>(NilValue.Instance);
    }
}
