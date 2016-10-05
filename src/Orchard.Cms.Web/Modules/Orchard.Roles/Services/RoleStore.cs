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
        private RolesDocument _roles;
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

        public async Task<RolesDocument> GetRolesAsync()
        {
            return await _memoryCache.GetOrCreateAsync(Key, async (entry) =>
            {
                _roles = await _session.QueryAsync<RolesDocument>().FirstOrDefault();

                if (_roles == null)
                {
                    _roles = new RolesDocument();
                    UpdateRoles();
                }

                entry.ExpirationTokens.Add(_signal.GetToken(Key));

                return _roles;
            });
        }

        public void UpdateRoles()
        {
            if (_roles != null)
            {
                _roles.Serial++;
                _session.Save(_roles);
                _signal.SignalToken(Key);
            }
        }

        public async Task<IEnumerable<string>> GetRoleNamesAsync()
        {
            var roles = await GetRolesAsync();
            return roles.Roles.Select(x => x.RoleName).OrderBy(x => x).ToList();
        }

        #region IRoleStore<Role>
        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = await GetRolesAsync();
            roles.Roles.Add(role);
            UpdateRoles();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

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
            UpdateRoles();

            return IdentityResult.Success;
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roles = await GetRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.RoleName == roleId);
            return role;
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var roles = await GetRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.NormalizedRoleName == normalizedRoleName);
            return role;
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.NormalizedRoleName);
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName.ToUpperInvariant());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            return Task.FromResult(role.RoleName);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.NormalizedRoleName = normalizedName;
            UpdateRoles();

            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.RoleName = roleName;
            UpdateRoles();

            return Task.CompletedTask;
        }

        public Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            UpdateRoles();

            return Task.FromResult(IdentityResult.Success);
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
            UpdateRoles();

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
            UpdateRoles();

            return Task.CompletedTask;
        }

        #endregion
    }
}
