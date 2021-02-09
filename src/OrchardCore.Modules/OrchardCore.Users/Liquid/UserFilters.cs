using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Users.Liquid
{
    public static class UserFilters
    {
        public static async ValueTask<FluidValue> UsersById(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var session = context.Services.GetRequiredService<ISession>();

            if (input.Type == FluidValues.Array)
            {
                // List of user ids
                var userIds = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await session.Query<User, UserIndex>(x => x.UserId.IsIn(userIds)).ListAsync(), ctx.Options);
            }
            else
            {
                var userId = input.ToStringValue();

                return FluidValue.Create(await session.Query<User, UserIndex>(x => x.UserId == userId).FirstOrDefaultAsync(), ctx.Options);
            }
        }

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

        public static async ValueTask<FluidValue> HasPermission(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var auth = context.Services.GetRequiredService<IAuthorizationService>();
            var permissionProviders = context.Services.GetRequiredService<IEnumerable<IPermissionProvider>>();

            var clearance = false;
            var permissionName = arguments["permission"].Or(arguments.At(0)).ToStringValue();
            var resource = arguments["resource"].Or(arguments.At(1)).ToObjectValue();

            Permission permission = null;

            foreach (var provider in permissionProviders)
            {
                var permissions = await provider.GetPermissionsAsync();

                permission = permissions.FirstOrDefault(p => p.Name == permissionName);

                if (permission != null)
                {
                    break;
                }
            }

            if (permission is Permission && input.ToObjectValue() is ClaimsPrincipal principal)
            {
                clearance = await auth.AuthorizeAsync(principal, permission, resource);
            }

            return clearance ? BooleanValue.True : BooleanValue.False;
        }

        public static ValueTask<FluidValue> IsInRole(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var ret = false;

            var claimName = arguments["name"].Or(arguments.At(0)).ToStringValue();

            var context = (LiquidTemplateContext)ctx;
            var optionsAccessor = context.Services.GetRequiredService<IOptions<IdentityOptions>>();
            var roleClaimType = optionsAccessor.Value.ClaimsIdentity.RoleClaimType;

            if (input.ToObjectValue() is ClaimsPrincipal principal)
            {
                ret = principal.Claims.Any(claim => claim.Type == roleClaimType && claim.Value.Equals(claimName, StringComparison.OrdinalIgnoreCase));
            }

            return new ValueTask<FluidValue>(ret ? BooleanValue.True : BooleanValue.False);
        }

        public static async ValueTask<FluidValue> UserEmail(FluidValue input, FilterArguments args, TemplateContext ctx)
        {

            if (input.ToObjectValue() is ClaimsPrincipal claimsPrincipal)
            {
                // Todo: Use 'IdentityOptions.ClaimsIdentity.EmailClaimType' that will be supported in a future version.
                // Currently the 'DefaultUserClaimsPrincipalFactory' also uses an hardcoded "email" for the claim type.
                var email = claimsPrincipal.FindFirstValue("email") ?? claimsPrincipal.FindFirstValue(ClaimTypes.Email);

                if (email == null)
                {
                    return NilValue.Instance;
                }

                return FluidValue.Create(email, ctx.Options);
            }

            if (input.ToObjectValue() is IUser user)
            {
                var context = (LiquidTemplateContext)ctx;
                var userManager = context.Services.GetRequiredService<UserManager<IUser>>();

                return FluidValue.Create(await userManager.GetEmailAsync(user), ctx.Options);
            }

            return NilValue.Instance;
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
