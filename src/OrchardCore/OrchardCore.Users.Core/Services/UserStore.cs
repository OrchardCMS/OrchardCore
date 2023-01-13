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
using OrchardCore.Security.Services;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Users.Services
{
    public class UserStore :
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
        private readonly IRoleService _roleService;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public UserStore(ISession session,
            ILookupNormalizer keyNormalizer,
            IUserIdGenerator userIdGenerator,
            ILogger<UserStore> logger,
            IEnumerable<IUserEventHandler> handlers,
            IRoleService roleService,
            IDataProtectionProvider dataProtectionProvider)
        {
            _session = session;
            _keyNormalizer = keyNormalizer;
            _userIdGenerator = userIdGenerator;
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
            Handlers = handlers;
            _roleService = roleService;
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

        public async Task<IdentityResult> CreateAsync(IUser user, CancellationToken cancellationToken = default)
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

        public async Task<IdentityResult> DeleteAsync(IUser user, CancellationToken cancellationToken = default)
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

        public async Task<IUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _session.Query<User, UserIndex>(u => u.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<IUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            return await _session.Query<User, UserIndex>(u => u.NormalizedUserName == normalizedUserName).FirstOrDefaultAsync();
        }

        public Task<string> GetNormalizedUserNameAsync(IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.NormalizedUserName);
            }

            return Task.FromResult<string>(null);
        }

        public Task<string> GetUserIdAsync(IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.UserId);
            }

            return Task.FromResult<string>(null);
        }

        public Task<string> GetUserNameAsync(IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(IUser user, string normalizedName, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.NormalizedUserName = normalizedName;
            }

            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(IUser user, string userName, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.UserName = userName;
            }

            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IUser user, CancellationToken cancellationToken = default)
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

        public Task<string> GetPasswordHashAsync(IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.PasswordHash);
            }

            return Task.FromResult<string>(null);
        }

        public Task SetPasswordHashAsync(IUser user, string passwordHash, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.PasswordHash = passwordHash;
            }

            return Task.CompletedTask;
        }

        public Task<bool> HasPasswordAsync(IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.PasswordHash != null);
            }

            return Task.FromResult(false);
        }

        #endregion IUserPasswordStore<IUser>

        #region ISecurityStampValidator<IUser>

        public Task SetSecurityStampAsync(IUser user, string stamp, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.SecurityStamp = stamp;
            }

            return Task.CompletedTask;
        }

        public Task<string> GetSecurityStampAsync(IUser user, CancellationToken cancellationToken = default)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.SecurityStamp);
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

            if (user is User u)
            {
                u.Email = email;
            }

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.Email);
            }

            return Task.FromResult<string>(null);
        }

        public Task<bool> GetEmailConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.EmailConfirmed);
            }

            return Task.FromResult(false);
        }

        public Task SetEmailConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.EmailConfirmed = confirmed;
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

            if (user is User u)
            {
                return Task.FromResult(u.NormalizedEmail);
            }

            return Task.FromResult<string>(null);
        }

        public Task SetNormalizedEmailAsync(IUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.NormalizedEmail = normalizedEmail;
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

            if (user is User u)
            {
                var roleNames = await _roleService.GetRoleNamesAsync();

                if (!roleNames.Any(r => NormalizeKey(r) == normalizedRoleName))
                {
                    throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
                }

                u.RoleNames.Add(normalizedRoleName);
            }
        }

        public async Task RemoveFromRoleAsync(IUser user, string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                var roleNames = await _roleService.GetRoleNamesAsync();

                if (!roleNames.Any(r => NormalizeKey(r) == normalizedRoleName))
                {
                    throw new InvalidOperationException($"Role {normalizedRoleName} does not exist.");
                }

                u.RoleNames.Remove(normalizedRoleName);
            }
        }

        public Task<IList<string>> GetRolesAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.RoleNames);
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

            if (user is User u)
            {
                return Task.FromResult(u.RoleNames.Contains(normalizedRoleName, StringComparer.OrdinalIgnoreCase));
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

            if (user is User u)
            {
                if (u.LoginInfos.Any(i => i.LoginProvider == login.LoginProvider))
                {
                    throw new InvalidOperationException($"Provider {login.LoginProvider} is already linked for {user.UserName}");
                }

                u.LoginInfos.Add(login);
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

            if (user is User u)
            {
                return Task.FromResult(u.LoginInfos);
            }

            return Task.FromResult<IList<UserLoginInfo>>(new List<UserLoginInfo>());
        }

        public Task RemoveLoginAsync(IUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u && u.LoginInfos != null)
            {
                var item = u.LoginInfos.FirstOrDefault(c => c.LoginProvider == loginProvider && c.ProviderKey == providerKey);
                if (item != null)
                {
                    u.LoginInfos.Remove(item);
                }
            }

            return Task.CompletedTask;
        }

        #endregion IUserLoginStore<IUser>

        #region IUserClaimStore<IUser>

        public Task<IList<Claim>> GetClaimsAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is not User u)
            {
                return Task.FromResult<IList<Claim>>(new List<Claim>());
            }

            return Task.FromResult<IList<Claim>>(u.UserClaims.Select(x => x.ToClaim()).ToList());
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

            if (user is User u)
            {
                foreach (var claim in claims)
                {
                    u.UserClaims.Add(new UserClaim { ClaimType = claim.Type, ClaimValue = claim.Value });
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

            if (user is User u)
            {
                foreach (var userClaim in u.UserClaims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type))
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

            if (user is User u)
            {
                foreach (var claim in claims)
                {
                    foreach (var userClaim in u.UserClaims.Where(uc => uc.ClaimValue == claim.Value && uc.ClaimType == claim.Type).ToList())
                    {
                        u.UserClaims.Remove(userClaim);
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
        public Task<string> GetTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
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

        public Task RemoveTokenAsync(IUser user, string loginProvider, string name, CancellationToken cancellationToken = default)
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
            if (userToken != null && user is User u)
            {
                u.UserTokens.Remove(userToken);
            }

            return Task.CompletedTask;
        }

        public Task SetTokenAsync(IUser user, string loginProvider, string name, string value, CancellationToken cancellationToken = default)
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

            if (userToken == null && user is User u)
            {
                userToken = new UserToken
                {
                    LoginProvider = loginProvider,
                    Name = name
                };

                u.UserTokens.Add(userToken);
            }

            // Encrypt the token.
            if (userToken != null)
            {
                userToken.Value = _dataProtectionProvider.CreateProtector(TokenProtector).Protect(value);
            }

            return Task.CompletedTask;
        }

        private static UserToken GetUserToken(IUser user, string loginProvider, string name)
        {
            if (user is User u)
            {
                return u.UserTokens.FirstOrDefault(ut => ut.LoginProvider == loginProvider && ut.Name == name);
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

            if (user is User u)
            {
                return Task.FromResult(u.AccessFailedCount);
            }

            return Task.FromResult(0);
        }

        public Task<bool> GetLockoutEnabledAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.IsLockoutEnabled);
            }

            return Task.FromResult(false);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u && u.LockoutEndUtc.HasValue)
            {
                return Task.FromResult<DateTimeOffset?>(u.LockoutEndUtc.Value.ToUniversalTime());
            }

            return Task.FromResult<DateTimeOffset?>(null);
        }

        public Task<int> IncrementAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                return Task.FromResult(u.AccessFailedCount++);
            }

            return Task.FromResult(0);
        }

        public Task ResetAccessFailedCountAsync(IUser user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.AccessFailedCount = 0;
            }

            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(IUser user, bool enabled, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                u.IsLockoutEnabled = enabled;
            }

            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (user is User u)
            {
                if (lockoutEnd.HasValue)
                {
                    u.LockoutEndUtc = lockoutEnd.Value.UtcDateTime;
                }
                else
                {
                    u.LockoutEndUtc = null;
                }
            }

            return Task.CompletedTask;
        }

        #endregion IUserLockoutStore<IUser>
    }
}
