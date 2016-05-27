using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Orchard.DependencyInjection;
using Orchard.Settings;

namespace Orchard.Security.AuthorizationHandlers
{
    /// <summary>
    /// This authorization handler validates any permission when the user is the site owner.
    /// </summary>
    [ScopedComponent(typeof(IAuthorizationHandler))]
    public class SuperUserHandler : IAuthorizationHandler
    {
        private readonly ISiteService _siteService;

        public SuperUserHandler(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task HandleAsync(AuthorizationContext context)
        {
            if (context?.User?.Identity?.Name == null)
            {
                return;
            }

            var superUser = (await _siteService.GetSiteSettingsAsync()).SuperUser;

            if (String.Equals(context.User.Identity.Name, superUser, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var requirement in context.Requirements.OfType<PermissionRequirement>())
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
