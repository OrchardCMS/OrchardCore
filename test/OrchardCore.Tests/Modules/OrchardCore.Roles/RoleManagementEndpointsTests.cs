using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
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
    public async Task DeleteAsync_MissingRole_ReturnsNotFound()
    {
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByIdAsync("missing")).ReturnsAsync((IRole)null);

        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();

        var result = await RoleManagementEndpoints.DeleteAsync(
            new DefaultHttpContext { RequestServices = services },
            "missing",
            CreateAuthorizationService(),
            roleManager.Object,
            CreateRoleService().Object,
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status404NotFound, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        roleManager.Verify(manager => manager.DeleteAsync(It.IsAny<IRole>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ExistingRole_ReturnsNoContent()
    {
        var role = new Role { RoleName = "Editors" };
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByIdAsync("Editors")).ReturnsAsync(role);
        roleManager.Setup(manager => manager.DeleteAsync(role)).ReturnsAsync(IdentityResult.Success);

        var result = await RoleManagementEndpoints.DeleteAsync(
            new DefaultHttpContext(),
            "Editors",
            CreateAuthorizationService(),
            roleManager.Object,
            CreateRoleService().Object,
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status204NoContent, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
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
                new Permission("ManageUsers") { Description = LocalizationSource.Create("Manage users"), Category = "Users" },
                new Permission("ManageRoles") { Description = LocalizationSource.Create("Manage roles"), Category = "Roles" },
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

    [Fact]
    public async Task CreateAsync_PermissionCallerDoesNotHold_ReturnsForbidden()
    {
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();

        var result = await RoleManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            new RoleManagementEndpoints.RoleCreateRequest
            {
                RoleName = "Escalated",
                PermissionNames = ["ManageContent", "ManageTenants"],
            },
            CreateAuthorizationService("ManageRoles", "ManageContent"),
            roleManager.Object,
            CreateRoleService().Object,
            CreatePermissionCatalog("ManageContent", "ManageTenants"),
            CreateLocalizer());

        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, problem.StatusCode);
        Assert.Contains("ManageTenants", problem.ProblemDetails.Detail);
        Assert.DoesNotContain("ManageContent", problem.ProblemDetails.Detail);
        roleManager.Verify(manager => manager.CreateAsync(It.IsAny<IRole>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NewPermissionCallerDoesNotHold_ReturnsForbiddenAndKeepsRole()
    {
        var role = new Role { RoleName = "Editors", RoleDescription = "Before", RoleClaims = [RoleClaim.Create("ManageContent")] };
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByIdAsync("Editors")).ReturnsAsync(role);

        var result = await RoleManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            "Editors",
            new RoleManagementEndpoints.RoleUpdateRequest { RoleDescription = "After", PermissionNames = ["ManageContent", "ManageTenants"] },
            CreateAuthorizationService("ManageRoles", "ManageContent"),
            roleManager.Object,
            CreateRoleService().Object,
            CreatePermissionCatalog("ManageContent", "ManageTenants"),
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status403Forbidden, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal("Before", role.RoleDescription);
        Assert.Equal(["ManageContent"], role.RoleClaims.Select(claim => claim.ClaimValue));
        roleManager.Verify(manager => manager.UpdateAsync(It.IsAny<IRole>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ExistingPermissionCallerDoesNotHold_CanRemoveIt()
    {
        var role = new Role { RoleName = "Editors", RoleClaims = [RoleClaim.Create("ManageContent"), RoleClaim.Create("ManageTenants")] };
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.FindByIdAsync("Editors")).ReturnsAsync(role);
        roleManager.Setup(manager => manager.UpdateAsync(role)).ReturnsAsync(IdentityResult.Success);

        var result = await RoleManagementEndpoints.UpdateAsync(
            new DefaultHttpContext(),
            "Editors",
            new RoleManagementEndpoints.RoleUpdateRequest { PermissionNames = ["ManageContent"] },
            CreateAuthorizationService("ManageRoles"),
            roleManager.Object,
            CreateRoleService().Object,
            CreatePermissionCatalog("ManageContent", "ManageTenants"),
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status200OK, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        Assert.Equal(["ManageContent"], role.RoleClaims.Select(claim => claim.ClaimValue));
    }

    [Fact]
    public async Task CreateAsync_SecurityCriticalPermissionCallerHolds_Succeeds()
    {
        var critical = new Permission("Critical", LocalizationSource.Create("Critical"), [], true);
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();
        roleManager.Setup(manager => manager.CreateAsync(It.IsAny<IRole>())).ReturnsAsync(IdentityResult.Success);

        var result = await RoleManagementEndpoints.CreateAsync(
            new DefaultHttpContext(),
            new RoleManagementEndpoints.RoleCreateRequest { RoleName = "Operators", PermissionNames = ["Critical"] },
            CreateAuthorizationService("ManageRoles", "Critical"),
            roleManager.Object,
            CreateRoleService().Object,
            new PermissionCatalog([critical], new Dictionary<string, string>(), new Dictionary<string, string>()),
            CreateLocalizer());

        Assert.Equal(StatusCodes.Status201Created, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
    }

    private static IAuthorizationService CreateAuthorizationService(params string[] heldPermissions)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<System.Security.Claims.ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((System.Security.Claims.ClaimsPrincipal _, object _, IEnumerable<IAuthorizationRequirement> requirements) =>
                requirements.OfType<PermissionRequirement>().All(requirement => heldPermissions.Contains(requirement.Permission.Name))
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());

        return authorizationService.Object;
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

    private static PermissionCatalog CreatePermissionCatalog(params string[] permissionNames)
        => new(
            permissionNames.Select(name => new Permission(name)),
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
