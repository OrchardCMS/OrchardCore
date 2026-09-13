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
using OrchardCore.Users.Endpoints.Management;
using OrchardCore.Users.Models;

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
