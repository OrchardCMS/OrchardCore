using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Endpoints.Management;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Users;

public class UserManagementEndpointsTests
{
    [Fact]
    public void ToResponse_SortsRolesAndOmitsSensitiveFields()
    {
        var response = UserManagementEndpoints.ToResponse(new User
        {
            UserId = "user-1",
            UserName = "alice",
            Email = "alice@example.com",
            PhoneNumber = "555-0100",
            PasswordHash = "hash",
            SecurityStamp = "stamp",
            ResetToken = "reset-token",
            EmailConfirmed = true,
            IsEnabled = true,
            IsLockoutEnabled = true,
            AccessFailedCount = 2,
            RoleNames = ["Editor", "Author"],
        });

        var json = JsonSerializer.Serialize(response);

        Assert.Equal(["Author", "Editor"], response.RoleNames);
        Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("securityStamp", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("resetToken", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_InvalidPassword_SavesNothing()
    {
        var user = new User { UserId = "alice", UserName = "alice", Email = "old@example.test", IsEnabled = true };
        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync("alice")).ReturnsAsync(user);
        var validator = new Mock<IPasswordValidator<IUser>>();
        validator.Setup(value => value.ValidateAsync(userManager.Object, user, "weak"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Too weak." }));
        userManager.Object.PasswordValidators.Add(validator.Object);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        var provider = services.BuildServiceProvider();
        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var result = await UserManagementEndpoints.UpdateAsync(
            new DefaultHttpContext { RequestServices = provider },
            "alice",
            new UserManagementEndpoints.UserUpdateRequest { Email = "new@example.test", Password = "weak" },
            userManager.Object,
            Mock.Of<IUserService>(),
            authorization.Object,
            provider);

        Assert.Equal(StatusCodes.Status400BadRequest, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal("old@example.test", user.Email);
        userManager.Verify(manager => manager.UpdateAsync(It.IsAny<IUser>()), Times.Never);
    }

    [Theory]
    [InlineData(false, StatusCodes.Status400BadRequest, 0)]
    [InlineData(true, StatusCodes.Status200OK, 1)]
    public async Task DeleteAsync_Administrator_KeepsTheOnlyEnabledOne(bool hasOtherAdministrator, int expectedStatus, int expectedDeletions)
    {
        var user = new User { UserId = "admin", UserName = "admin", IsEnabled = true, RoleNames = [OrchardCoreConstants.Roles.Administrator] };
        var administrators = new List<IUser> { user };
        if (hasOtherAdministrator)
        {
            administrators.Add(new User { UserId = "other", UserName = "other", IsEnabled = true });
        }

        var userManager = CreateUserManager();
        userManager.Setup(manager => manager.FindByIdAsync("admin")).ReturnsAsync(user);
        userManager.Setup(manager => manager.GetUsersInRoleAsync(OrchardCoreConstants.Roles.Administrator)).ReturnsAsync(administrators);
        userManager.Setup(manager => manager.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);
        var roleService = new Mock<IRoleService>();
        roleService.Setup(service => service.IsAdminRoleAsync(OrchardCoreConstants.Roles.Administrator)).ReturnsAsync(true);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddSingleton(roleService.Object);
        var provider = services.BuildServiceProvider();
        var authorization = new Mock<IAuthorizationService>();
        authorization
            .Setup(service => service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());
        var localizer = new Mock<IStringLocalizer<AdminController>>();
        localizer.Setup(value => value[It.IsAny<string>()]).Returns<string>(name => new LocalizedString(name, name));

        var result = await UserManagementEndpoints.DeleteAsync(
            new DefaultHttpContext { RequestServices = provider },
            "admin",
            userManager.Object,
            authorization.Object,
            provider,
            localizer.Object);

        Assert.Equal(expectedStatus, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        userManager.Verify(manager => manager.DeleteAsync(user), Times.Exactly(expectedDeletions));
    }

    [Fact]
    public async Task UpdateUserRolesAsync_SoleEnabledAdministrator_PreservesAdministratorRole()
    {
        var user = new User
        {
            UserId = "admin",
            UserName = "admin",
            IsEnabled = true,
            RoleNames = [OrchardCoreConstants.Roles.Administrator],
        };
        var roles = new List<string>(user.RoleNames);
        var roleStore = new Mock<IUserRoleStore<IUser>>();
        roleStore
            .Setup(store => store.GetRolesAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => roles);
        roleStore
            .Setup(store => store.RemoveFromRoleAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<IUser, string, CancellationToken>((_, role, _) => roles.RemoveAll(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase)))
            .Returns(Task.CompletedTask);

        var userStore = Mock.Of<IUserStore<IUser>>();
        var userManager = new Mock<UserManager<IUser>>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IUser>(),
            Array.Empty<IUserValidator<IUser>>(),
            Array.Empty<IPasswordValidator<IUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            NullLogger<UserManager<IUser>>.Instance);
        userManager
            .Setup(manager => manager.GetUsersInRoleAsync(OrchardCoreConstants.Roles.Administrator))
            .ReturnsAsync([user]);
        userManager
            .Setup(manager => manager.UpdateSecurityStampAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var administratorRole = Mock.Of<IRole>(role => role.RoleName == OrchardCoreConstants.Roles.Administrator);
        var roleService = new Mock<IRoleService>();
        roleService.Setup(service => service.GetRolesAsync()).ReturnsAsync([administratorRole]);
        roleService.Setup(service => service.IsAdminRoleAsync(OrchardCoreConstants.Roles.Administrator)).Returns(() => ValueTask.FromResult(true));
        var services = new ServiceCollection()
            .AddSingleton(roleStore.Object)
            .AddSingleton(roleService.Object)
            .BuildServiceProvider();
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        await UserManagementEndpoints.UpdateUserRolesAsync(
            services,
            userManager.Object,
            user,
            [],
            new ClaimsPrincipal(new ClaimsIdentity()),
            authorizationService.Object);

        Assert.Contains(OrchardCoreConstants.Roles.Administrator, user.RoleNames);
        roleStore.Verify(
            store => store.RemoveFromRoleAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_UnauthorizedCurrentRole_PreservesRole()
    {
        var user = new User
        {
            UserId = "user-1",
            UserName = "alice",
            IsEnabled = true,
            RoleNames = ["Protected"],
        };
        var roles = new List<string>(user.RoleNames);
        var roleStore = new Mock<IUserRoleStore<IUser>>();
        roleStore
            .Setup(store => store.GetRolesAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => roles);
        roleStore
            .Setup(store => store.RemoveFromRoleAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<IUser, string, CancellationToken>((_, role, _) => roles.RemoveAll(value => string.Equals(value, role, StringComparison.OrdinalIgnoreCase)))
            .Returns(Task.CompletedTask);

        var userManager = CreateUserManager();
        var protectedRole = Mock.Of<IRole>(role => role.RoleName == "Protected");
        var roleService = new Mock<IRoleService>();
        roleService.Setup(service => service.GetRolesAsync()).ReturnsAsync([protectedRole]);
        roleService.Setup(service => service.IsAdminRoleAsync("Protected")).Returns(() => ValueTask.FromResult(false));
        roleService.Setup(service => service.IsSystemRoleAsync("Protected")).Returns(() => ValueTask.FromResult(false));
        var services = new ServiceCollection()
            .AddSingleton(roleStore.Object)
            .AddSingleton(roleService.Object)
            .BuildServiceProvider();
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                protectedRole,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        await UserManagementEndpoints.UpdateUserRolesAsync(
            services,
            userManager.Object,
            user,
            [],
            new ClaimsPrincipal(new ClaimsIdentity()),
            authorizationService.Object);

        Assert.Contains("Protected", user.RoleNames);
        roleStore.Verify(
            store => store.RemoveFromRoleAsync(user, It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task MatchesCreateRequestAsync_IdenticalUser_ReturnsTrue()
    {
        var user = new User
        {
            UserName = "alice",
            Email = "alice@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
            IsEnabled = true,
            RoleNames = ["Editor", "Author"],
        };
        var request = new UserManagementEndpoints.UserCreateRequest
        {
            UserName = "alice",
            Email = "ALICE@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
            IsEnabled = true,
            RoleNames = ["Author", "Editor"],
        };

        var matches = await UserManagementEndpoints.MatchesCreateRequestAsync(
            user,
            request,
            CreateUserManager().Object);

        Assert.True(matches);
    }

    private static Mock<UserManager<IUser>> CreateUserManager()
    {
        var userStore = Mock.Of<IUserStore<IUser>>();
        return new Mock<UserManager<IUser>>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<IUser>(),
            Array.Empty<IUserValidator<IUser>>(),
            Array.Empty<IPasswordValidator<IUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            NullLogger<UserManager<IUser>>.Instance);
    }
}
