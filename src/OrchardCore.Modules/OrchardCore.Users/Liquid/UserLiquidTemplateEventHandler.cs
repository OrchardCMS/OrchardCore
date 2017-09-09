using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Http;
using OrchardCore.Liquid;
using System.Security.Claims;

namespace OrchardCore.Users.Liquid
{
    public class UserLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLiquidTemplateEventHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task PopulateValuesAsync(IDictionary<string, object> values)
        {
            values.Add("User", _httpContextAccessor.HttpContext.User);
            values.Add("User.Identity", _httpContextAccessor.HttpContext.User.Identity);
            return Task.CompletedTask;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            var user = _httpContextAccessor.HttpContext.User;

            context.MemberAccessStrategy.Register<ClaimsPrincipal>();
            context.MemberAccessStrategy.Register<ClaimsIdentity>();
            context.LocalScope.SetValue("User", user);

            return Task.CompletedTask;
        }
    }
}
