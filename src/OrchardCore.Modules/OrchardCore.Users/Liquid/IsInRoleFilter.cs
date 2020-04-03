using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Permissions;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class IsInRoleFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var ret = false;
            var claimType = ClaimTypes.Role;
            var claimName = arguments["name"].Or(arguments.At(0)).ToStringValue();

            if (input.ToObjectValue() is ClaimsPrincipal principal)
            {
                ret = principal.HasClaim(claimType, claimName);
            }
            
            return new ValueTask<FluidValue>(ret ? BooleanValue.True : BooleanValue.False);
        }
    }
}
