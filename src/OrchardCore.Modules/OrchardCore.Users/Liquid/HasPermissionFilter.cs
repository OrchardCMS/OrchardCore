using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.Liquid;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users.Liquid;

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
            if (user != null && arguments.Count > 0)
            {
                var permissionName = arguments["permission"].Or(arguments.At(0)).ToStringValue();

                if (string.IsNullOrWhiteSpace(permissionName))
                {
                    return BooleanValue.False;
                }

                var permission = new Permission(permissionName);

                if (arguments.Count > 1)
                {
                    var resource = arguments["resource"].Or(arguments.At(1)).ToObjectValue();

                    if (resource != null)
                    {
                        if (!string.IsNullOrEmpty(permissionName) &&
                            await _authorizationService.AuthorizeAsync(user, permission, resource))
                        {
                            return BooleanValue.True;
                        }
                    }
                }

                if (await _authorizationService.AuthorizeAsync(user, permission))
                {
                    return BooleanValue.True;
                }
            }
        }

        return BooleanValue.False;
    }
}
