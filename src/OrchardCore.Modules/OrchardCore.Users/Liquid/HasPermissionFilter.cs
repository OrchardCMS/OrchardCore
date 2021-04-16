using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Liquid;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users.Liquid
{
    public class HasPermissionFilter : ILiquidFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HasPermissionFilter(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            if (input.ToObjectValue() is LiquidUserAccessor)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    var permissionName = arguments["permission"].Or(arguments.At(0)).ToStringValue();
                    var resource = arguments["resource"].Or(arguments.At(1)).ToObjectValue();

                    if (!String.IsNullOrEmpty(permissionName) &&
                        await _authorizationService.AuthorizeAsync(user, new Permission(permissionName), resource))
                    {
                        return BooleanValue.True;
                    }
                }
            }

            return BooleanValue.False;
        }
    }
}
