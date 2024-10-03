using OrchardCore.Roles;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles;

public class PermissionHandlerHelperTests
{
    [Theory]
    [InlineData("AllowAnonymous", true, true)]
    [InlineData("AllowAnonymous", false, true)]
    [InlineData("AllowAuthenticated", true, true)]
    [InlineData("AllowAuthenticated", false, false)]
    public async Task GrantsRolesPermissions(string required, bool authenticated, bool success)
    {
        // Arrange
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission(required), authenticated: authenticated);

        var permissionHandler = CreatePermissionHandler(
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Anonymous,
                RoleClaims =
                [
                    RoleClaim.Create("AllowAnonymous"),
                ]
            },
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Authenticated,
                RoleClaims =
                [
                    RoleClaim.Create("AllowAuthenticated"),
                ]
            }
        );

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.Equal(success, context.HasSucceeded);
    }

    [Fact]
    public async Task DoNotRevokeExistingGrants()
    {
        // Arrange
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission("Required"), ["Other"], true);

        var permissionHandler = CreatePermissionHandler();

        await context.SuccessAsync("Required");

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task GrantsInheritedPermissions()
    {
        // Arrange
        var level2 = new Permission("Implicit2");
        var level1 = new Permission("Implicit1", "Foo", new[] { level2 });
        var required = new Permission("Required", "Foo", new[] { level1 });

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required);

        var permissionHandler = CreatePermissionHandler(
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Anonymous,
                RoleClaims =
                [
                    RoleClaim.Create("Implicit2"),
                ]
            }
        );

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData("AllowAnonymous", true)]
    [InlineData("AllowAuthenticated", true)]
    public async Task IsCaseInsensitive(string required, bool authenticated)
    {
        // Arrange
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission(required), authenticated: authenticated);

        var permissionHandler = CreatePermissionHandler(
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Anonymous,
                RoleClaims =
                [
                    RoleClaim.Create("aLlOwAnOnYmOuS"),
                ]
            },
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Authenticated,
                RoleClaims =
                [
                    RoleClaim.Create("aLlOwAuThEnTiCaTeD"),
                ]
            }
        );

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    private static RolesPermissionsHandler CreatePermissionHandler(params Role[] roles)
    {
        var roleManager = RolesMockHelper.MockRoleManager<IRole>();

        foreach (var role in roles)
        {
            roleManager.Setup(m => m.FindByNameAsync(role.RoleName)).ReturnsAsync(role);
        }

        var permissionGrantingService = new DefaultPermissionGrantingService();
        return new RolesPermissionsHandler(roleManager.Object, permissionGrantingService);
    }
}
