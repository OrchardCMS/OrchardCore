using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Environment.Cache;
using Orchard.Roles.Models;
using Orchard.Security;
using Orchard.Security.Services;
using YesSql.Core.Services;

namespace Orchard.Roles.Services
{
    public class RoleStore : IRoleClaimStore<Role>, IRoleProvider
    {
        private const string Key = "RolesManager.Roles";

        private readonly ISession _session;
        private readonly ISignal _signal;
        private readonly IMemoryCache _memoryCache;

        public RoleStore(ISession session, IMemoryCache memoryCache, ISignal signal)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
        }

        public void Dispose()
        {
        }

        public Task<RolesDocument> GetRolesAsync()
        {
            return _memoryCache.GetOrCreateAsync(Key, async (entry) =>
            {
                var roles = await _session.QueryAsync<RolesDocument>().FirstOrDefault();

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

        #region IRoleStore<Role>
        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = await GetRolesAsync();
            roles.Roles.Add(role);
            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (String.Equals(role.NormalizedRoleName, "ANONYMOUS") ||
                String.Equals(role.NormalizedRoleName, "AUTHENTICATED"))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Can't delete system roles." });
            }

            var roles = await GetRolesAsync();
            roles.Roles.Remove(role);
            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.RoleName == roleId);
            return role;
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roles = await GetRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.NormalizedRoleName == normalizedRoleName);
            return role;
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedRoleName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName.ToUpperInvariant());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.NormalizedRoleName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.RoleName = roleName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = await GetRolesAsync();
            var existingRole = roles.Roles.FirstOrDefault(x => x.RoleName == role.RoleName);
            roles.Roles.Remove(existingRole);
            roles.Roles.Add(role);

            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        #endregion

        #region IRoleClaimStore<Role>
        public Task AddClaimAsync(Role role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            role.RoleClaims.Add(new RoleClaim { ClaimType = claim.Type, ClaimValue = claim.Value } );

            return Task.CompletedTask;
        }

        public Task<IList<Claim>> GetClaimsAsync(Role role, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult<IList<Claim>>(role.RoleClaims.Select(x => x.ToClaim()).ToList());
        }

        public Task RemoveClaimAsync(Role role, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            role.RoleClaims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

            return Task.CompletedTask;
        }

        #endregion
    }
}
