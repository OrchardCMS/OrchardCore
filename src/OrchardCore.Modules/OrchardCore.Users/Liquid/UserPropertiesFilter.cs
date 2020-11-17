using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Liquid;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Liquid
{
    public class UserPropertiesFilter : ILiquidFilter
    {
        private readonly UserManager<IUser> _userManager;

        public UserPropertiesFilter(UserManager<IUser> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments args, TemplateContext context)
        {
            if (input.ToObjectValue() is ClaimsPrincipal claimsPrincipal)
            {
                var user = await _userManager.GetUserAsync(claimsPrincipal);
                return FluidValue.Create(((User)user).Properties);
            }

            return NilValue.Instance;
        }
    }
}
