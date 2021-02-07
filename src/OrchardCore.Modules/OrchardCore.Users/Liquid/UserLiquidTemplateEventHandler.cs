using System.Security.Claims;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Http;
using OrchardCore.Liquid;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Liquid
{
    public class UserLiquidTemplateEventHandler : ILiquidTemplateEventHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserLiquidTemplateEventHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task RenderingAsync(TemplateContext context)
        {
            var user = _httpContextAccessor.HttpContext.User;

            context.MemberAccessStrategy.Register<User>(
                nameof(User.UserId),
                nameof(User.UserName),
                nameof(User.NormalizedUserName),
                nameof(User.Email),
                nameof(User.NormalizedEmail),
                nameof(User.EmailConfirmed),
                nameof(User.IsEnabled),
                nameof(User.RoleNames),
                nameof(User.Properties)
            );

            context.MemberAccessStrategy.Register<ClaimsPrincipal>();
            context.MemberAccessStrategy.Register<ClaimsIdentity>();
            context.SetValue("User", user);

            return Task.CompletedTask;
        }
    }
}
