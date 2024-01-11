using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class IsInRoleFilter : ILiquidFilter
    {
        private readonly IdentityOptions _identityOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IsInRoleFilter(IOptions<IdentityOptions> identityOptions, IHttpContextAccessor httpContextAccessor)
        {
            _identityOptions = identityOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
        {
            if (input.ToObjectValue() is LiquidUserAccessor)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    var claimName = arguments["name"].Or(arguments.At(0)).ToStringValue();
                    var roleClaimType = _identityOptions.ClaimsIdentity.RoleClaimType;

                    if (user.Claims.Any(claim => claim.Type == roleClaimType && claim.Value.Equals(claimName, StringComparison.OrdinalIgnoreCase)))
                    {
                        return new ValueTask<FluidValue>(BooleanValue.True);
                    }
                }
            }

            return new ValueTask<FluidValue>(BooleanValue.False);
        }
    }
}
