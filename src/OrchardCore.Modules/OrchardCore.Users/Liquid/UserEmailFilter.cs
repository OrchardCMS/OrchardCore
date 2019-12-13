using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Liquid;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OrchardCore.Users.Liquid
{
    public class UserEmailFilter : ILiquidFilter
    {
        private readonly UserManager<IUser> _userManager;

        public UserEmailFilter(UserManager<IUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments args, TemplateContext context)
        {
            if (input.ToObjectValue() is ClaimsPrincipal claimsPrincipal)
            {
                return FluidValue.Create(await _userManager.GetEmailAsync(await _userManager.GetUserAsync(claimsPrincipal)));
            }

            if (input.ToObjectValue() is IUser user)
            {
                return FluidValue.Create(await _userManager.GetEmailAsync(user));
            }

            return NilValue.Instance;
        }
    }
}
