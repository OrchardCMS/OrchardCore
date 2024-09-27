using OrchardCore.Roles.Core;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

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

        var permissionHandler = GetRolesPermissionHandler(true);

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task StandardRolesShouldNotGrantPermissions()
    {
        // Arrange
        var adminRolePermission = new Claim("role", "Editor");
        var required = new Permission("Required", "Foo");

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, [adminRolePermission], true);

        var permissionHandler = GetRolesPermissionHandler(false);

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    public static RolesPermissionHandler GetRolesPermissionHandler(bool userIsAdmin)
    {
        var options = new Mock<IOptions<IdentityOptions>>();

        options.Setup(x => x.Value).Returns(new IdentityOptions());

        var roleService = new Mock<IRoleService>();

        roleService.Setup(x => x.IsAdminRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(userIsAdmin);

        var permissionHandler = new RolesPermissionHandler(roleService.Object, options.Object);

        return permissionHandler;
    }
}
