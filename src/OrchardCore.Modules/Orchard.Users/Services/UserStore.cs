using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Orchard.Security.Services;
using Orchard.Users.Indexes;
using Orchard.Users.Models;
using YesSql;

namespace Orchard.Users.Services
{
    public class UserStore :
        IUserStore<User>,
        IUserRoleStore<User>,
        IUserPasswordStore<User>,
        IUserEmailStore<User>,
        IUserSecurityStampStore<User>
    {
        private readonly ISession _session;
        private readonly IRoleProvider _roleProvider;
        private readonly ILookupNormalizer _normalizeKey;

        public UserStore(ISession session,
            IRoleProvider roleProvider,
            ILookupNormalizer normalizeKey)
        {
            _session = session;
            _roleProvider = roleProvider;
            _normalizeKey = normalizeKey;
        }

        public void Dispose()
        {
        }

        #region IUserStore<User>
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _session.Save(user);

            return Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _session.Delete(user);

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

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            int id;
            if (!int.TryParse(userId, out id))
            {
                return Task.FromResult<User>(null);
            }

            return _session.GetAsync<User>(id);
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _session.Query<User, UserIndex>(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _session.Save(user);

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

        #endregion

        #region IUserPasswordStore<User>
        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash != null);
        }

        #endregion

        #region ISecurityStampValidator<User>
        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.SecurityStamp);
        }
        #endregion

        #region IUserEmailStore<User>
        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.Email = email;

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return _session.Query<User, UserIndex>(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedEmail);
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.NormalizedEmail = normalizedEmail;

            return Task.CompletedTask;
        }

        #endregion

        #region IUserRoleStore<User>
        public async Task AddToRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var roleNames = await _roleProvider.GetRoleNamesAsync();
            var roleName = roleNames?.FirstOrDefault(r => _normalizeKey.Normalize(r) == normalizedRoleName);

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
            }

            user.RoleNames.Add(roleName);
            _session.Save(roleName);
        }

        public async Task RemoveFromRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var roleNames = await _roleProvider.GetRoleNamesAsync();
            var roleName = roleNames?.FirstOrDefault(r => _normalizeKey.Normalize(r) == normalizedRoleName);

            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
            }

            user.RoleNames.Remove(roleName);
            _session.Save(roleName);
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult<IList<string>>(user.RoleNames);
        }

        public Task<bool> IsInRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(normalizedRoleName));
            }

            return Task.FromResult(user.RoleNames.Contains(normalizedRoleName, StringComparer.OrdinalIgnoreCase));
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var roleNames = await _roleProvider.GetRoleNamesAsync();
            var roleName = roleNames?.FirstOrDefault(r => _normalizeKey.Normalize(r) == normalizedRoleName);

            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var users = await _session.Query<User>().ListAsync();
                return users.Where(u => u.RoleNames.Contains(roleName)).ToList();
            }

            return new List<User>();
        }
        #endregion
    }
}
