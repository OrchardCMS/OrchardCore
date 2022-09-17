using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Security.Services;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Services
{
    public class RoleBaseUserStore : UserStoreBase
    {
        private readonly IRoleService _roleService;

        public RoleBaseUserStore(ISession session,
            ILookupNormalizer keyNormalizer,
            IUserIdGenerator userIdGenerator,
            ILogger<RoleBaseUserStore> logger,
            IEnumerable<IUserEventHandler> handlers,
            IDataProtectionProvider dataProtectionProvider,
            IRoleService roleService)
            : base(session, keyNormalizer, userIdGenerator, logger, handlers, dataProtectionProvider)
        {
            _roleService = roleService;
        }

        protected override async Task AddToRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roleNames = await _roleService.GetRoleNamesAsync();

            if (!roleNames.Any(r => NormalizeKey(r) == normalizedRoleName))
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
            }

            await base.AddToRoleAsync(user, normalizedRoleName, cancellationToken);
        }

        protected override async Task RemoveFromRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roleNames = await _roleService.GetRoleNamesAsync();

            if (!roleNames.Any(r => NormalizeKey(r) == normalizedRoleName))
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
            }

            await base.RemoveFromRoleAsync(user, normalizedRoleName, cancellationToken);
        }
    }
}
