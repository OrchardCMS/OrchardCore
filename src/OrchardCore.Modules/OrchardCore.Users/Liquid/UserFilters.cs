using System.Security.Claims;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Liquid;
using OrchardCore.Roles;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users.Liquid;

public static class UserFilters
{
    public static async ValueTask<FluidValue> HasClaim(FluidValue input, FilterArguments arguments, TemplateContext ctx)
    {
        if (input.ToObjectValue() is LiquidUserAccessor)
        {
            var context = (LiquidTemplateContext)ctx;
            var httpContextAccessor = context.Services.GetRequiredService<IHttpContextAccessor>();
            var systemRoleNameProvider = context.Services.GetRequiredService<ISystemRoleNameProvider>();

            var user = httpContextAccessor.HttpContext?.User;
            if (user != null)
            {
                var claimType = arguments["type"].Or(arguments.At(0)).ToStringValue();
                var claimName = arguments["name"].Or(arguments.At(1)).ToStringValue();

                if (user.HasClaim(claimType, claimName))
                {
                    return BooleanValue.True;
                }

                // The following if condition was added in 2.1 for backward compatibility. It should be removed in v3 and documented as a breaking change.
                // The change log should state the following:
                // The `Administrator` role no longer registers permission-based claims by default during login. This means that directly checking for specific claims in Liquid, such as:
                //
                // ```liquid
                // {% assign isAuthorized = User | has_claim: "Permission", "AccessAdminPanel" %}
                // ```
                //
                // will return `false` for administrators, even though they still have full access. Non-admin users, however, may return `true` if they have the claim. 
                // it's important to use the `has_permission` filter for permission checks going forward:
                //
                // ```liquid
                // {% assign isAuthorized = User | has_permission: "AccessAdminPanel" %}
                // ```
                if (string.Equals(claimType, Permission.ClaimType, StringComparison.OrdinalIgnoreCase) &&
                    user.IsInRole(await systemRoleNameProvider.GetAdminRoleAsync()))
                {
                    var logger = context.Services.GetRequiredService<ILogger<Startup>>();

                    logger.LogWarning("The tenant is using the 'has_claim' Liquid filter for Permission claims '{ClaimName}', which will break in the next major release of OrchardCore; please use 'has_permission: \"{ClaimName}\"' instead.", claimName, claimName);

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
