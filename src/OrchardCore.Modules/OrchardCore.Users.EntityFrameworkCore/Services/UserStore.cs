using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrchardCore.Users.EntityFrameworkCore.Models;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.EntityFrameworkCore.Services
{
    public class UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim> :
        Microsoft.AspNetCore.Identity.EntityFrameworkCore.UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>,
        IUserEmailStore<IUser>,
        IUserStore<IUser>,
        IUserPasswordStore<IUser>,
        IUserSecurityStampStore<IUser>
        where TUser : User<TKey>, new()
        where TRole : Role<TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>, new()
        where TUserRole : IdentityUserRole<TKey>, new()
        where TUserLogin : IdentityUserLogin<TKey>, new()
        where TUserToken : IdentityUserToken<TKey>, new()
        where TRoleClaim : RoleClaim<TKey>, new()
    {

        public UserStore(TContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }

        #region IUserStore<IUser> implementation
        Task<string> IUserStore<IUser>.GetUserIdAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.GetUserIdAsync(ToStoreUser(user), cancellationToken);
        }

        Task<string> IUserStore<IUser>.GetUserNameAsync(IUser user, CancellationToken cancellationToken) => base.GetUserNameAsync(ToStoreUser(user), cancellationToken);

        async Task IUserStore<IUser>.SetUserNameAsync(IUser user, string userName, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetUserNameAsync(convertedUser, userName, cancellationToken);
            SyncChanges(convertedUser,user);
        }

        Task<string> IUserStore<IUser>.GetNormalizedUserNameAsync(IUser user, CancellationToken cancellationToken) => base.GetNormalizedUserNameAsync(ToStoreUser(user), cancellationToken);

        async Task IUserStore<IUser>.SetNormalizedUserNameAsync(IUser user, string normalizedName, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetNormalizedUserNameAsync(convertedUser, normalizedName, cancellationToken);
            SyncChanges(convertedUser, user);
        }

        Task<IdentityResult> IUserStore<IUser>.CreateAsync(IUser user, CancellationToken cancellationToken) => base.CreateAsync(ToStoreUser(user), cancellationToken);

        async Task<IdentityResult> IUserStore<IUser>.UpdateAsync(IUser user, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            var result = await base.UpdateAsync(convertedUser, cancellationToken);
            SyncChanges(convertedUser, user);
            return result;
        }

        Task<IdentityResult> IUserStore<IUser>.DeleteAsync(IUser user, CancellationToken cancellationToken) => base.DeleteAsync(ToStoreUser(user), cancellationToken);

        async Task<IUser> IUserStore<IUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var result = await base.FindByIdAsync(userId, cancellationToken);
            return ToStoreUser(result);
        }


        async Task<IUser> IUserStore<IUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var result = await base.FindByNameAsync(normalizedUserName, cancellationToken);
            return ToStoreUser(result);
        }
        #endregion
        


        #region IUserEmailStore<IUser> implementation
        async Task IUserEmailStore<IUser>.SetEmailAsync(IUser user, string email, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetEmailAsync(convertedUser, email, cancellationToken);
            SyncChanges(convertedUser, user);
        }

        Task<string> IUserEmailStore<IUser>.GetEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.GetEmailAsync(ToStoreUser(user), cancellationToken);
        }

        Task<bool> IUserEmailStore<IUser>.GetEmailConfirmedAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.GetEmailConfirmedAsync(ToStoreUser(user), cancellationToken);
        }

        async Task IUserEmailStore<IUser>.SetEmailConfirmedAsync(IUser user, bool confirmed, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetEmailConfirmedAsync(convertedUser, confirmed, cancellationToken);
            SyncChanges(convertedUser, user);
        }

        async Task<IUser> IUserEmailStore<IUser>.FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var user = await base.FindByEmailAsync(normalizedEmail, cancellationToken);
            return ToStoreUser(user);
        }

        Task<string> IUserEmailStore<IUser>.GetNormalizedEmailAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.GetNormalizedEmailAsync(ToStoreUser(user), cancellationToken);
        }

        async Task IUserEmailStore<IUser>.SetNormalizedEmailAsync(IUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetNormalizedEmailAsync(convertedUser, normalizedEmail, cancellationToken);
            SyncChanges(convertedUser, user);
        }
        #endregion


        #region IUserPasswordStore<IUser> implementation
        public async Task SetPasswordHashAsync(IUser user, string passwordHash, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetPasswordHashAsync(convertedUser, passwordHash, cancellationToken);
            SyncChanges(convertedUser, user);
        }

        public Task<string> GetPasswordHashAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.GetPasswordHashAsync(ToStoreUser(user), cancellationToken);
        }

        public Task<bool> HasPasswordAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.HasPasswordAsync(ToStoreUser(user), cancellationToken);
        }
        #endregion

        #region IUserSecurityStampStore<IUser> implementation
        public virtual async Task SetSecurityStampAsync(IUser user, string stamp, CancellationToken cancellationToken)
        {
            var convertedUser = ToStoreUser(user);
            await base.SetSecurityStampAsync(convertedUser, stamp, cancellationToken);
            SyncChanges(convertedUser, user);
        }

        public Task<string> GetSecurityStampAsync(IUser user, CancellationToken cancellationToken)
        {
            return base.GetSecurityStampAsync(ToStoreUser(user), cancellationToken);
        }
        #endregion

        protected virtual TUser ToStoreUser(IUser user)
        {
            if (user == null)
                return null;
            if (user is User orchardUser)
            {
                return new TUser
                {
                    UserName = orchardUser.UserName,
                    NormalizedUserName = orchardUser.NormalizedUserName,
                    Email = orchardUser.Email,
                    NormalizedEmail = orchardUser.NormalizedEmail,
                    EmailConfirmed = orchardUser.EmailConfirmed,
                    PasswordHash = orchardUser.PasswordHash,
                    SecurityStamp = orchardUser.SecurityStamp
                };
            }
            return user is TUser identityUser
                ? identityUser
                : new TUser
                {
                    UserName = user.UserName,
                    NormalizedUserName = user.UserName
                };
        }

        protected virtual void SyncChanges(TUser source, IUser destination)
        {
            if (source == null || destination==null)
                return;
            if (destination is User orchardUser)
            {
                orchardUser.UserName = source.UserName;
                orchardUser.NormalizedUserName = source.NormalizedUserName;
                orchardUser.Email = source.Email;
                orchardUser.NormalizedEmail = source.NormalizedEmail;
                orchardUser.EmailConfirmed = source.EmailConfirmed;
                orchardUser.PasswordHash = source.PasswordHash;
                orchardUser.SecurityStamp = source.SecurityStamp;
            }
        }
    }
}