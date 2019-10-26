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
using YesSql;

namespace OrchardCore.Roles.Services
{
    public class RoleStore : IRoleClaimStore<IRole>, IQueryableRoleStore<IRole>
    {
        private const string Key = "RolesManager.Roles";

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;

        private RolesDocument _rolesDocument;

        public RoleStore(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache,
            IStringLocalizer<RoleStore> stringLocalizer,
            IServiceProvider serviceProvider,
            ILogger<RoleStore> logger)
        {
            _signal = signal;
            _session = session;
            _memoryCache = memoryCache;
            T = stringLocalizer;
            _serviceProvider = serviceProvider;
            Logger = logger;
        }

        public ILogger Logger { get; }

        public IStringLocalizer<RoleStore> T;

        public void Dispose()
        {
        }

        public IQueryable<IRole> Roles => GetRolesAsync().GetAwaiter().GetResult().Roles.AsQueryable();

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public async Task<RolesDocument> LoadRolesAsync()
        {
            return _rolesDocument = _rolesDocument ?? await _session.Query<RolesDocument>().FirstOrDefaultAsync() ?? new RolesDocument();
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        private async Task<RolesDocument> GetRolesAsync()
        {
            if (!_memoryCache.TryGetValue<RolesDocument>(Key, out var document))
            {
                var changeToken = _signal.GetToken(Key);

                if (_rolesDocument != null)
                {
                    _session.Detach(_rolesDocument);
                }

                document = await _session.Query<RolesDocument>().FirstOrDefaultAsync();

                if (document != null)
                {
                    _session.Detach(document);

                    foreach (var role in document.Roles)
                    {
                        role.IsReadonly = true;
                    }
                }
                else
                {
                    document = new RolesDocument();
                }

                _memoryCache.Set(Key, document, changeToken);
            }

            return document;
        }

        private void UpdateRoles(RolesDocument roles)
        {
            roles.Serial++;
            _session.Save(roles);
            _signal.DeferredSignalToken(Key);
        }

        #region IRoleStore<IRole>
        public async Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            var roles = await LoadRolesAsync();
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

            var roleToRemove = (Role)role;

            if (roleToRemove.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            if (String.Equals(roleToRemove.NormalizedRoleName, "ANONYMOUS") ||
                String.Equals(roleToRemove.NormalizedRoleName, "AUTHENTICATED"))
            {
                return IdentityResult.Failed(new IdentityError { Description = T["Can't delete system roles."] });
            }

            var roleRemovedEventHandlers = _serviceProvider.GetRequiredService<IEnumerable<IRoleRemovedEventHandler>>();
            await roleRemovedEventHandlers.InvokeAsync(x => x.RoleRemovedAsync(roleToRemove.RoleName), Logger);

            var roles = await LoadRolesAsync();
            roleToRemove = roles.Roles.FirstOrDefault(r => r.RoleName == roleToRemove.RoleName);
            roles.Roles.Remove(roleToRemove);

            UpdateRoles(roles);

            return IdentityResult.Success;
        }

        public async Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var roles = await LoadRolesAsync();
            var role = roles.Roles.FirstOrDefault(x => x.RoleName == roleId);
            return role;
        }

        public async Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var roles = await LoadRolesAsync();
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

            if (((Role)role).IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
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

            if (((Role)role).IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
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

            if (((Role)role).IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var roles = await LoadRolesAsync();
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

            if (((Role)role).IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
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

            if (((Role)role).IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            ((Role)role).RoleClaims.RemoveAll(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

            return Task.CompletedTask;
        }

        #endregion
    }
}
