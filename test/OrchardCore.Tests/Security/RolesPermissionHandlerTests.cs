using OrchardCore.Roles.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Security;

public class RolesPermissionHandlerTests
{
    [Fact]
    public async Task RolesWithFullAccessShouldAutoGrantPermissions()
    {
        // Arrange
        var adminRolePermission = new Claim("role", "Administrator");
        var required = new Permission("Required", "Foo");

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, [adminRolePermission], true);

        var permissionHandler = new RolesPermissionHandler(new RoleTrackerTest(["Administrator"]));

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task RolesWithNoFullAccessShouldNotGrantPermissions()
    {
        // Arrange
        var adminRolePermission = new Claim("role", "Administrator");
        var required = new Permission("Required", "Foo");

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, [adminRolePermission], true);

        var permissionHandler = new RolesPermissionHandler(new RoleTrackerTest());

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }
}
