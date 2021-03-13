using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Liquid;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users.Liquid
{
    public class HasPermissionFilter : ILiquidFilter
    {
        private readonly IAuthorizationService _authorizationService;

        public HasPermissionFilter(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            var clearance = false;
            var permissionName = arguments["permission"].Or(arguments.At(0)).ToStringValue();
            var resource = arguments["resource"].Or(arguments.At(1)).ToObjectValue();

            if (!String.IsNullOrEmpty(permissionName) && input.ToObjectValue() is ClaimsPrincipal principal)
            {
                clearance = await _authorizationService.AuthorizeAsync(principal, new Permission(permissionName), resource);
            }

            return clearance ? BooleanValue.True : BooleanValue.False;
        }
    }
}
