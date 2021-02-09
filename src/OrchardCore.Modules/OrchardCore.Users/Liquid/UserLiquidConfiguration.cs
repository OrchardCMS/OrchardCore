using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Liquid
{
    public class UserLiquidConfiguration : IConfigureOptions<TemplateOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLiquidConfiguration(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Configure(TemplateOptions options)
        {
            var user = _httpContextAccessor.HttpContext.User;

            options.SetValue("User", user);
        }
    }
}
