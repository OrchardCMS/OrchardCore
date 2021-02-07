using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email;
using OrchardCore.Users;
using OrchardCore.Users.Services;

namespace OrchardCore.Tests.OrchardCore.Users
{
    public static class UsersMockHelper
    {
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var identityOptions = new IdentityOptions();
            identityOptions.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+";
            identityOptions.User.RequireUniqueEmail = true;

            var mgr = new Mock<UserManager<TUser>>(store.Object, Options.Create(identityOptions), null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>(new IdentityErrorDescriber()));
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            return mgr;
        }

        public static Mock<RoleManager<TRole>> MockRoleManager<TRole>() where TRole : class
        {
            var store = new Mock<IRoleStore<TRole>>().Object;
            var roles = new List<IRoleValidator<TRole>>();
            roles.Add(new RoleValidator<TRole>());

            return new Mock<RoleManager<TRole>>(store, roles, new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null);
        }
    }
}
