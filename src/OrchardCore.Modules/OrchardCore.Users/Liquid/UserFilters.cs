using System.Security.Claims;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Roles;

namespace OrchardCore.Users.Liquid;

public static class UserFilters
{
    public static async ValueTask<FluidValue> HasClaim(FluidValue input, FilterArguments arguments, TemplateContext ctx)
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

                var systemRoleNameProvider = context.Services.GetService<ISystemRoleNameProvider>();

                if (systemRoleNameProvider != null)
                {
                    // Administrator users do not register individual permissions during login.
                    // However, they are designed to automatically have all application permissions granted.
                    var identityOptions = context.Services.GetRequiredService<IOptions<IdentityOptions>>().Value;

                    if (user.HasClaim(identityOptions.ClaimsIdentity.RoleClaimType, await systemRoleNameProvider.GetAdminRoleAsync()))
                    {
                        return BooleanValue.True;
                    }
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
