using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Services
{
    public class UserStoreBase :
        IUserClaimStore<IUser>,
        IUserRoleStore<IUser>,
        IUserPasswordStore<IUser>,
        IUserEmailStore<IUser>,
        IUserSecurityStampStore<IUser>,
        IUserLoginStore<IUser>,
        IUserLockoutStore<IUser>,
        IUserAuthenticationTokenStore<IUser>
    {
        private const string TokenProtector = "OrchardCore.UserStore.Token";

        private readonly ISession _session;
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly IUserIdGenerator _userIdGenerator;
        private readonly ILogger _logger;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public UserStoreBase(ISession session,
            ILookupNormalizer keyNormalizer,
            IUserIdGenerator userIdGenerator,
            ILogger<UserStoreBase> logger,
            IEnumerable<IUserEventHandler> handlers,
            IDataProtectionProvider dataProtectionProvider)
        {
            _session = session;
            _keyNormalizer = keyNormalizer;
            _userIdGenerator = userIdGenerator;
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
            Handlers = handlers;
        }

        public IEnumerable<IUserEventHandler> Handlers { get; private set; }

        public void Dispose()
        {
        }

        public string NormalizeKey(string key)
        {
            return _keyNormalizer == null ? key : _keyNormalizer.NormalizeName(key);
        }

        #region IUserStore<IUser>

        public async Task<IdentityResult> CreateAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is not User newUser)
            {
                throw new ArgumentException("Expected a User instance.", nameof(user));
            }

            var newUserId = newUser.UserId;

            if (String.IsNullOrEmpty(newUserId))
            {
                // Due to database collation we normalize the userId to lower invariant.
                newUserId = _userIdGenerator.GenerateUniqueId(user).ToLowerInvariant();
            }

            try
            {
                var attempts = 10;

                while (await _session.QueryIndex<UserIndex>(x => x.UserId == newUserId).CountAsync() != 0)
                {
                    if (attempts-- == 0)
                    {
                        throw new ApplicationException("Couldn't generate a unique user id. Too many attempts.");
                    }

                    newUserId = _userIdGenerator.GenerateUniqueId(user).ToLowerInvariant();
                }

                newUser.UserId = newUserId;

                var context = new UserCreateContext(user);

                await Handlers.InvokeAsync((handler, context) => handler.CreatingAsync(context), context, _logger);

                if (context.Cancel)
                {
                    return IdentityResult.Failed();
                }

                _session.Save(user);
                await _session.SaveChangesAsync();
                await Handlers.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while creating a new user.");

                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                var context = new UserDeleteContext(user);
                await Handlers.InvokeAsync((handler, context) => handler.DeletingAsync(context), context, _logger);

                if (context.Cancel)
                {
                    return IdentityResult.Failed();
                }

                _session.Delete(user);
                await _session.SaveChangesAsync();
                await Handlers.InvokeAsync((handler, context) => handler.DeletedAsync(context), context, _logger);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while deleting a user.");

                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }

        public async Task<IUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _session.Query<User, UserIndex>(u => u.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<IUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _session.Query<User, UserIndex>(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedUserNameAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.NormalizedUserName);
            }

            return Task.FromResult<string>(null);
        }

        public Task<string> GetUserIdAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.UserId);
            }

            return Task.FromResult<string>(null);
        }

        public Task<string> GetUserNameAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(IUser user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.NormalizedUserName);
            }

            return Task.FromResult<string>(null);
        }

        public Task SetUserNameAsync(IUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.UserName = userName;
            }

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            try
            {
                var context = new UserUpdateContext(user);
                await Handlers.InvokeAsync((handler, context) => handler.UpdatingAsync(context), context, _logger);

                if (context.Cancel)
                {
                    return IdentityResult.Failed();
                }

                _session.Save(user);
                await _session.SaveChangesAsync();
                await Handlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), context, _logger);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while updating a user.");

                return IdentityResult.Failed();
            }

            return IdentityResult.Success;
        }

        #endregion IUserStore<IUser>

        #region IUserPasswordStore<IUser>

        public Task<string> GetPasswordHashAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.PasswordHash);
            }

            return Task.FromResult<string>(null);
        }

        public Task SetPasswordHashAsync(IUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.PasswordHash = passwordHash;
            }

            return Task.CompletedTask;
        }

        public Task<bool> HasPasswordAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.PasswordHash != null);
            }

            return Task.FromResult(false);
        }

        #endregion IUserPasswordStore<IUser>

        #region ISecurityStampValidator<IUser>

        public Task SetSecurityStampAsync(IUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.SecurityStamp = stamp;
            }

            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(IUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.SecurityStamp);
            }

            return Task.FromResult<string>(null);
        }

        #endregion ISecurityStampValidator<IUser>

        #region IUserEmailStore<IUser>

        public Task SetEmailAsync(IUser user, string email, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.Email = email;
            }

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.Email);
            }

            return Task.FromResult<string>(null);
        }

        public Task<bool> GetEmailConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.EmailConfirmed);
            }

            return Task.FromResult(false);
        }

        public Task SetEmailConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.EmailConfirmed = confirmed;
            }

            return Task.CompletedTask;
        }

        public async Task<IUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await _session.Query<User, UserIndex>(u => u.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.NormalizedEmail);
            }

            return Task.FromResult<string>(null);
        }

        public Task SetNormalizedEmailAsync(IUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.NormalizedEmail = normalizedEmail;
            }

            return Task.CompletedTask;
        }

        #endregion IUserEmailStore<IUser>

        #region IUserRoleStore<IUser>

        public async Task AddToRoleAsync(IUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                await AddToRoleAsync(su, normalizedRoleName, cancellationToken);
            }
        }

        protected virtual Task AddToRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            user.RoleNames.Add(normalizedRoleName);

            return Task.CompletedTask;
        }

        public async Task RemoveFromRoleAsync(IUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                await RemoveFromRoleAsync(su, normalizedRoleName, cancellationToken);
            }
        }

        protected virtual Task RemoveFromRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            user.RoleNames.Remove(normalizedRoleName);

            return Task.CompletedTask;
        }

        public Task<IList<string>> GetRolesAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.RoleNames);
            }

            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<bool> IsInRoleAsync(IUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(normalizedRoleName));
            }

            if (user is User su)
            {
                return Task.FromResult(su.RoleNames.Contains(normalizedRoleName, StringComparer.OrdinalIgnoreCase));
            }

            return Task.FromResult(false);
        }

        public async Task<IList<IUser>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(normalizedRoleName))
            {
                throw new ArgumentNullException(nameof(normalizedRoleName));
            }

            var users = await _session.Query<User, UserByRoleNameIndex>(u => u.RoleName == normalizedRoleName).ListAsync();
            return users == null ? new List<IUser>() : users.ToList<IUser>();
        }

        #endregion IUserRoleStore<IUser>

        #region IUserLoginStore<IUser>

        public Task AddLoginAsync(IUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }

            if (user is User su)
            {
                if (su.LoginInfos.Any(i => i.LoginProvider == login.LoginProvider))
                {
                    throw new InvalidOperationException($"Provider {login.LoginProvider} is already linked for {user.UserName}");
                }

                su.LoginInfos.Add(login);
            }

            return Task.CompletedTask;
        }

        public async Task<IUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return await _session.Query<User, UserByLoginInfoIndex>(u => u.LoginProvider == loginProvider && u.ProviderKey == providerKey).FirstOrDefaultAsync();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.LoginInfos);
            }

            return Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());
        }

        public Task RemoveLoginAsync(IUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su && su.LoginInfos != null)
            {
                var item = su.LoginInfos.FirstOrDefault(c => c.LoginProvider == loginProvider && c.ProviderKey == providerKey);
                if (item != null)
                {
                    su.LoginInfos.Remove(item);
                }
            }

            return Task.CompletedTask;
        }

        #endregion IUserLoginStore<IUser>

        #region IUserClaimStore<IUser>

        public async Task<IList<Claim>> GetClaimsAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is not User su)
            {
                return new List<Claim>();
            }

            return await GetClaimsAsync(su, cancellationToken);
        }

        protected virtual Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult<IList<Claim>>(user.UserClaims.Select(x => x.ToClaim()).ToList());
        }

        public Task AddClaimsAsync(IUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (user is User su)
            {
                foreach (var claim in claims)
                {
                    su.UserClaims.Add(new UserClaim { ClaimType = claim.Type, ClaimValue = claim.Value });
                }
            }

            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(IUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            if (newClaim == null)
            {
                throw new ArgumentNullException(nameof(newClaim));
            }

            if (user is User su)
            {
                foreach (var userClaim in su.UserClaims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type))
                {
                    userClaim.ClaimValue = newClaim.Value;
                    userClaim.ClaimType = newClaim.Type;
                }
            }

            return Task.CompletedTask;
        }

        public Task RemoveClaimsAsync(IUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (user is User su)
            {
                foreach (var claim in claims)
                {
                    foreach (var userClaim in su.UserClaims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList())
                    {
                        su.UserClaims.Remove(userClaim);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public async Task<IList<IUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }

            var users = await _session.Query<User, UserByClaimIndex>(uc => uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value).ListAsync();

            return users.Cast<IUser>().ToList();
        }

        #endregion IUserClaimStore<IUser>

        #region IUserAuthenticationTokenStore
        public Task<string> GetTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(loginProvider))
            {
                throw new ArgumentException("The login provider cannot be null or empty.", nameof(loginProvider));
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name cannot be null or empty.", nameof(name));
            }

            var userToken = GetUserToken(user, loginProvider, name);
            if (userToken != null)
            {
                return Task.FromResult(_dataProtectionProvider.CreateProtector(TokenProtector).Unprotect(userToken.Value));
            }

            return Task.FromResult<string>(null);
        }

        public Task RemoveTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(loginProvider))
            {
                throw new ArgumentException("The login provider cannot be null or empty.", nameof(loginProvider));
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name cannot be null or empty.", nameof(name));
            }

            var userToken = GetUserToken(user, loginProvider, name);
            if (userToken != null && user is User su)
            {
                su.UserTokens.Remove(userToken);
            }

            return Task.CompletedTask;
        }

        public Task SetTokenAsync(IUser user, string loginProvider, string name, string value, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (String.IsNullOrEmpty(loginProvider))
            {
                throw new ArgumentException("The login provider cannot be null or empty.", nameof(loginProvider));
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The name cannot be null or empty.", nameof(name));
            }

            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The value cannot be null or empty.", nameof(value));
            }

            var userToken = GetUserToken(user, loginProvider, name);

            if (userToken == null && user is User su)
            {
                userToken = new UserToken
                {
                    LoginProvider = loginProvider,
                    Name = name
                };

                su.UserTokens.Add(userToken);
            }

            // Encrypt the token
            userToken.Value = _dataProtectionProvider.CreateProtector(TokenProtector).Protect(value);

            return Task.CompletedTask;
        }

        private static UserToken GetUserToken(IUser user, string loginProvider, string name)
        {
            if (user is User su)
            {
                return su.UserTokens.FirstOrDefault(ut => ut.LoginProvider == loginProvider && ut.Name == name);
            }

            return null;
        }
        #endregion

        #region IUserLockoutStore<IUser>

        public Task<int> GetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.AccessFailedCount);
            }

            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.IsLockoutEnabled);
            }

            return Task.FromResult(false);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su && su.LockoutEndUtc.HasValue)
            {
                return Task.FromResult<DateTimeOffset?>(su.LockoutEndUtc.Value.ToUniversalTime());
            }

            return Task.FromResult<DateTimeOffset?>(null);
        }

        public Task<int> IncrementAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                return Task.FromResult(su.AccessFailedCount++);
            }

            return Task.FromResult(0);
        }

        public Task ResetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.AccessFailedCount = 0;
            }

            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                su.IsLockoutEnabled = enabled;
            }

            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User su)
            {
                if (lockoutEnd.HasValue)
                {
                    su.LockoutEndUtc = lockoutEnd.Value.UtcDateTime;
                }
                else
                {
                    su.LockoutEndUtc = null;
                }
            }

            return Task.CompletedTask;
        }

        #endregion IUserLockoutStore<IUser>
    }
}
