using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class UserEmailFilter : ILiquidFilter
    {
        private readonly UserManager<IUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserEmailFilter(UserManager<IUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments args, LiquidTemplateContext ctx)
        {
            var value = input.ToObjectValue();
            if (value is LiquidUserAccessor)
            {
                var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
                if (claimsPrincipal != null)
                {
                    // Todo: Use 'IdentityOptions.ClaimsIdentity.EmailClaimType' that will be supported in a future version.
                    // Currently the 'DefaultUserClaimsPrincipalFactory' also uses an hardcoded "email" for the claim type.
                    var email = claimsPrincipal.FindFirstValue("email") ?? claimsPrincipal.FindFirstValue(ClaimTypes.Email);
                    if (email != null)
                    {
                        return FluidValue.Create(email, ctx.Options);
                    }

                    return NilValue.Instance;
                }
            }

            if (value is IUser user)
            {
                return FluidValue.Create(await _userManager.GetEmailAsync(user), ctx.Options);
            }

            return NilValue.Instance;
        }
    }
}
