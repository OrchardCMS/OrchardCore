using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Roles.Controllers;
using OrchardCore.Roles.Endpoints.Management;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles;

public class RoleManagementEndpointsTests
{
    [Fact]
    public async Task CreateAsync_ExactRetryReturnsExistingRoleWithoutCreatingDuplicate()
    {
        var existing = new Role
        {
            RoleName = "Editors",
            RoleDescription = "Content editors",
            RoleClaims = [RoleClaim.Create("ManageContent")],
        };
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByNameAsync("Editors")).ReturnsAsync(existing);

        var result = await RoleManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            new RoleManagementEndpoints.RoleCreateRequest
            {
                RoleName = " Editors ",
                RoleDescription = "Content editors",
                PermissionNames = ["ManageContent"],
            },
            CreateAuthorizationService(),
            roleManager.Object,
            CreateRoleService().Object,
            CreatePermissionCatalog(),
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status201Created, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        roleManager.Verify(manager => manager.CreateAsync(It.IsAny<IRole>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ConflictingRetryReturnsConflict()
    {
        var existing = new Role
        {
            RoleName = "Editors",
            RoleDescription = "Content editors",
            RoleClaims = [RoleClaim.Create("ManageContent")],
        };
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByNameAsync("Editors")).ReturnsAsync(existing);

        var result = await RoleManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            new RoleManagementEndpoints.RoleCreateRequest
            {
                RoleName = "Editors",
                RoleDescription = "Different description",
                PermissionNames = ["ManageContent"],
            },
            CreateAuthorizationService(),
            roleManager.Object,
            CreateRoleService().Object,
            CreatePermissionCatalog(),
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        roleManager.Verify(manager => manager.CreateAsync(It.IsAny<IRole>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_MissingRoleReturnsSuccessfulConvergedResponse()
    {
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByIdAsync("missing")).ReturnsAsync((IRole)null);

        var result = await RoleManagementEndpoints.DeleteAsync(
            new DefaultHttpContext(),
            "missing",
            CreateAuthorizationService(),
            roleManager.Object,
            CreateRoleService().Object,
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        roleManager.Verify(manager => manager.DeleteAsync(It.IsAny<IRole>()), Times.Never);
    }

    [Fact]
    public async Task ToResponseAsync_AdminRoleMarksAllPermissionsEffective()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        var roleService = new Mock<IRoleService>();
        roleService.Setup(service => service.IsSystemRoleAsync(OrchardCoreConstants.Roles.Administrator)).ReturnsAsync(true);
        roleService.Setup(service => service.IsAdminRoleAsync(OrchardCoreConstants.Roles.Administrator)).ReturnsAsync(true);

        var permissionCatalog = new PermissionCatalog(
            [
                new Permission("ManageUsers") { Description = "Manage users", Category = "Users" },
                new Permission("ManageRoles") { Description = "Manage roles", Category = "Roles" },
            ],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ManageUsers"] = "OrchardCore.Users",
                ["ManageRoles"] = "OrchardCore.Roles",
            },
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ManageUsers"] = "Users",
                ["ManageRoles"] = "Roles",
            });

        var response = await RoleManagementEndpoints.ToResponseAsync(new Role
        {
            RoleName = OrchardCoreConstants.Roles.Administrator,
            RoleClaims = [RoleClaim.Create("ManageUsers")],
        }, authorizationService.Object, roleService.Object, permissionCatalog);

        Assert.True(response.IsSystemRole);
        Assert.True(response.IsAdminRole);
        Assert.Equal(2, response.PermissionCount);
        Assert.All(response.Permissions, permission => Assert.True(permission.IsEffective));
        Assert.True(response.Permissions.Single(permission => permission.Name == "ManageUsers").IsAssigned);
        Assert.False(response.Permissions.Single(permission => permission.Name == "ManageRoles").IsAssigned);
    }

    private static IAuthorizationService CreateAuthorizationService()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        return authorizationService.Object;
    }

    private static Mock<IRoleService> CreateRoleService()
    {
        var roleService = new Mock<IRoleService>();
        roleService.Setup(service => service.IsSystemRoleAsync(It.IsAny<string>())).ReturnsAsync(false);
        roleService.Setup(service => service.IsAdminRoleAsync(It.IsAny<string>())).ReturnsAsync(false);
        return roleService;
    }

    private static PermissionCatalog CreatePermissionCatalog()
        => new(
            [new Permission("ManageContent")],
            new Dictionary<string, string>(StringComparer.Ordinal),
            new Dictionary<string, string>(StringComparer.Ordinal));

    private static IStringLocalizer<AdminController> CreateLocalizer()
    {
        var localizer = new Mock<IStringLocalizer<AdminController>>();
        localizer.Setup(value => value[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        localizer.Setup(value => value[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string name, object[] arguments) => new LocalizedString(name, string.Format(name, arguments)));
        return localizer.Object;
    }
}
