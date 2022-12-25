using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Users;

namespace OrchardCore.Testing.Mocks;

public static partial class OrchardCoreMock
{
    public static SignInManager<TUser> CreateSignInManager<TUser>(UserManager<TUser> userManager = null) where TUser : class, IUser
    {
        var context = new Mock<HttpContext>();
        var manager = userManager ?? CreateUserManagerMock<TUser>().Object;

        var signInManager = new Mock<SignInManager<TUser>>(
            manager,
            new HttpContextAccessor { HttpContext = context.Object },
            Mock.Of<IUserClaimsPrincipalFactory<TUser>>(),
            null,
            null,
            null,
            null)
        {
            CallBase = true
        };

        signInManager
            .Setup(x => x.SignInAsync(It.IsAny<TUser>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        return signInManager.Object;
    }

    public static Mock<UserManager<TUser>> CreateUserManagerMock<TUser>() where TUser : class
    {
        var userStore = new Mock<IUserStore<TUser>>();
        var identityOptions = new IdentityOptions();
        identityOptions.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+";
        identityOptions.User.RequireUniqueEmail = true;

        var userManagerMock = new Mock<UserManager<TUser>>(
            userStore.Object,
            Options.Create(identityOptions),
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        userManagerMock.Object.UserValidators.Add(new UserValidator<TUser>(new IdentityErrorDescriber()));

        userManagerMock.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

        return userManagerMock;
    }

    public static RoleManager<TRole> CreateRoleManager<TRole>() where TRole : class
    {
        var roleStoreMock = new Mock<IRoleStore<TRole>>().Object;
        var rolesValidators = new List<IRoleValidator<TRole>>
        {
            new RoleValidator<TRole>()
        };

        var roleManagerMock = new Mock<RoleManager<TRole>>(
            roleStoreMock,
            rolesValidators,
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null);

        return roleManagerMock.Object;
    }
}
