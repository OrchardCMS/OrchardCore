using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// This authorization handler validates any permission when the user is the site owner.
    /// </summary>
    public class SuperUserHandler : IAuthorizationHandler
    {
        private readonly ISiteService _siteService;

        public SuperUserHandler(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            var userId = context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return;
            }

            var site = await _siteService.GetSiteSettingsAsync();

            if (String.Equals(userId, site.SuperUser, StringComparison.OrdinalIgnoreCase))
            {
                SucceedAllRequirements(context);
            }
        }

        private static void SucceedAllRequirements(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.Requirements.OfType<PermissionRequirement>())
            {
                context.Succeed(requirement);
            }
        }
    }
}
