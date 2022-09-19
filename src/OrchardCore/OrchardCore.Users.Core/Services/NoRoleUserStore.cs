using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Services
{
    public class NoRoleUserStore : UserStore
    {
        private readonly IServiceProvider _serviceProvider;
        private IEnumerable<IPermissionProvider> _permissionProviders;
        private ISiteService _siteService;

        public NoRoleUserStore(ISession session,
            ILookupNormalizer keyNormalizer,
            IUserIdGenerator userIdGenerator,
            ILogger<NoRoleUserStore> logger,
            IEnumerable<IUserEventHandler> handlers,
            IDataProtectionProvider dataProtectionProvider,
            IServiceProvider serviceProvider)
            : base(session, keyNormalizer, userIdGenerator, logger, handlers, dataProtectionProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var claims = new List<Claim>();

            _siteService ??= _serviceProvider.GetService<ISiteService>();
            _permissionProviders ??= _serviceProvider.GetServices<IPermissionProvider>();

            if (_siteService == null || _permissionProviders == null || !_permissionProviders.Any())
            {
                return claims;
            }

            var site = await _siteService.GetSiteSettingsAsync();

            if (String.Equals(site.SuperUser, user.UserId))
            {
                foreach (var permissionProvider in _permissionProviders)
                {
                    claims.AddRange(permissionProvider.GetDefaultStereotypes()
                        .Where(stereotype => stereotype.Name.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
                        .SelectMany(stereotype => stereotype.Permissions)
                        .Select(permission => new Claim(Permission.ClaimType, permission.Name)));
                }
            }

            return claims;
        }
    }
}
