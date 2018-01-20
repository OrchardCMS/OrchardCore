using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrchardCore.Roles.Models;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using YesSql;

namespace OrchardCore.Roles.Services
{
    public class RoleStore : IRoleClaimStore<IRole>, IRoleProvider
    {
        private const string Key = "RolesManager.Roles";

        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;

        public RoleStore(ISession session,
            IMemoryCache memoryCache,
            ISignal signal,
            IStringLocalizer<RoleStore> stringLocalizer,
            IServiceProvider serviceProvider,
            ILogger<RoleStore> logger)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            T = stringLocalizer;
            _session = session;
            _serviceProvider = serviceProvider;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public IStringLocalizer<RoleStore> T;

        public void Dispose()
        {
        }

        public Task<RolesDocument> GetRolesAsync()
        {
            return _memoryCache.GetOrCreateAsync(Key, async (entry) =>
            {
                var roles = await _session.Query<RolesDocument>().FirstOrDefaultAsync();

                if (roles == null)
                {
                    roles = new RolesDocument();
                    _session.Save(roles);
                }

                entry.ExpirationTokens.Add(_signal.GetToken(Key));

                return roles;
            });
        }

        public void UpdateRoles(RolesDocument roles)
        {
            roles.Serial++;
            _session.Save(roles);
            _memoryCache.Set(Key, roles);
        }

        public async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roles = await GetRolesAsync();
            return roles.Roles.Select(x => x.RoleName).OrderBy(x => x).ToList();
        }

        #region IRoleStore<IRole>
        public async Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = await GetRolesAsync();
            roles.Roles.Add((Role)role);
            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            var orchardRole = (Role)role;

            if (String.Equals(orchardRole.NormalizedRoleName, "ANONYMOUS") ||
                String.Equals(orchardRole.NormalizedRoleName, "AUTHENTICATED"))
            {
                return IdentityResult.Failed(new IdentityError { Description = T["Can't delete system roles."] });
            }

            var roleRemovedEventHandlers = _serviceProvider.GetRequiredService<IEnumerable<IRoleRemovedEventHandler>>();
            await roleRemovedEventHandlers.InvokeAsync(x => x.RoleRemovedAsync(orchardRole.RoleName), Logger);

            var roles = await GetRolesAsync();
            roles.Roles.Remove(orchardRole);
            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        public async Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.RoleName == roleId);
            return role;
        }

        public async Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.NormalizedRoleName == normalizedRoleName);
            return role;
        }

        public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(((Role)role).NormalizedRoleName);
        }

        public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName.ToUpperInvariant());
        }

        public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName);
        }

        public Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            ((Role)role).NormalizedRoleName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            ((Role)role).RoleName = roleName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = await GetRolesAsync();
            var existingRole = roles.Roles.FirstOrDefault(x => x.RoleName == role.RoleName);
            roles.Roles.Remove(existingRole);
            roles.Roles.Add((Role)role);

            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        #endregion

        #region IRoleClaimStore<IRole>
        public Task AddClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            ((Role)role).RoleClaims.Add(new RoleClaim { ClaimType = claim.Type, ClaimValue = claim.Value });

            return Task.CompletedTask;
        }

        public Task<IList<Claim>> GetClaimsAsync(IRole role, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult<IList<Claim>>(((Role)role).RoleClaims.Select(x => x.ToClaim()).ToList());
        }

        public Task RemoveClaimAsync(IRole role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            ((Role)role).RoleClaims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

            return Task.CompletedTask;
        }

        #endregion
    }
}
