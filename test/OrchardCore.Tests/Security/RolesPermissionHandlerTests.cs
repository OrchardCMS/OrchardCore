using OrchardCore.Roles.Core;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Security;

public class RolesPermissionHandlerTests
{
    [Fact]
    public async Task OwnerRolesShouldAutoGrantPermissions()
    {
        // Arrange
        var adminRolePermission = new Claim("role", "Administrator");
        var required = new Permission("Required", "Foo");

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, [adminRolePermission], true);

        var permissionHandler = GetRolesPermissionHandler("Administrator");

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task StandardRolesShouldNotGrantPermissions()
    {
        // Arrange
        var adminRolePermission = new Claim("role", "Administrator");
        var required = new Permission("Required", "Foo");

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, [adminRolePermission], true);

        var permissionHandler = GetRolesPermissionHandler();

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    public static RolesPermissionHandler GetRolesPermissionHandler(params string[] ownerRoles)
    {
        var options = new Mock<IOptions<IdentityOptions>>();

        options.Setup(x => x.Value).Returns(new IdentityOptions());

        var permissionHandler = new RolesPermissionHandler(new RoleTrackerStub(ownerRoles), options.Object);

        return permissionHandler;
    }
}
