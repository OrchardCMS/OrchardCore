using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Orchard.DependencyInjection;
using Orchard.Identity.Indexes;
using YesSql.Core.Services;

namespace Orchard.Identity
{
    public class RoleStore : IRoleStore<Role>
    {
        private readonly ISession _session;

        public RoleStore(ISession session)
        {
            _session = session;
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _session.Save(role);

            try
            {
                await _session.CommitAsync();
            }
            catch
            {
                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _session.Delete(role);

            try
            {
                await _session.CommitAsync();
            }
            catch
            {
                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int id;
            if (!int.TryParse(roleId, out id))
            {
                return Task.FromResult<Role>(null);
            }

            return _session.GetAsync<Role>(id);
        }

        public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.QueryAsync<Role, RoleIndex>(r => r.NormalizedRoleName == normalizedRoleName).FirstOrDefault();

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

            return Task.FromResult(role.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
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

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _session.Save(role);

            try
            {
                await _session.CommitAsync();
            }
            catch
            {
                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }
    }
}
