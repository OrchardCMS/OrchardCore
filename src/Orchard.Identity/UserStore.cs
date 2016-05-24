using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Orchard.Identity.Indexes;
using YesSql.Core.Services;

namespace Orchard.Identity
{
    public class UserStore :
        IUserStore<User>,
        IUserPasswordStore<User>
    {
        private readonly ISession _session;

        public UserStore(ISession session)
        {
            _session = session;
        }

        public void Dispose()
        {
        }

        #region IUserStore<User>
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _session.Save(user);

            return Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

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
            cancellationToken.ThrowIfCancellationRequested();

            int id;
            if(!int.TryParse(userId, out id))
            {
                return Task.FromResult<User>(null);
            }

            return _session.GetAsync<User>(id);
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.QueryAsync<User, UserIndex>(u => u.NormalizedUserName == normalizedUserName).FirstOrDefault();
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.UserName = userName;

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

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

        #region IUserPasswordStore<TUser>
        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.PasswordHash != null);
        }

        #endregion
    }
}
