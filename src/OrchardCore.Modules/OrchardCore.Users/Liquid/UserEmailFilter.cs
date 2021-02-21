using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Liquid;

namespace OrchardCore.Users.Liquid
{
    public class UserEmailFilter : ILiquidFilter
    {
        private readonly UserManager<IUser> _userManager;

        public UserEmailFilter(UserManager<IUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments args, TemplateContext ctx)
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

                return FluidValue.Create(await _userManager.GetEmailAsync(user), ctx.Options);
            }

            return NilValue.Instance;
        }
    }
}
