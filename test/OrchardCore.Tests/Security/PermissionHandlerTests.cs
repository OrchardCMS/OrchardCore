using OrchardCore.Security;
using OrchardCore.Security.AuthorizationHandlers;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Security;

public class PermissionHandlerTests
{
    [Theory]
    [InlineData("Allowed", true)]
    [InlineData("NotAllowed", false)]
    public async Task GrantsClaimsPermissions(string required, bool success)
    {
        // Arrange
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission(required), ["Allowed"], true);
        var permissionHandler = CreatePermissionHandler();

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
    public async Task DoNotHandleNonAuthenticated()
    {
        // Arrange
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission("Allowed"), ["Allowed"]);
        var permissionHandler = CreatePermissionHandler();

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task GrantsInheritedPermissions()
    {
        // Arrange
        var level2 = new Permission("Implicit2");
        var level1 = new Permission("Implicit1", "Foo", new[] { level2 });
        var required = new Permission("Required", "Foo", new[] { level1 });

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, ["Implicit2"], true);
        var permissionHandler = CreatePermissionHandler();

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task IsCaseInsensitive()
    {
        // Arrange
        var required = new Permission("required");

        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(required, ["ReQuIrEd"], true);
        var permissionHandler = CreatePermissionHandler();

        // Act
        await permissionHandler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    private static PermissionHandler CreatePermissionHandler()
    {
        var permissionGrantingService = new DefaultPermissionGrantingService();
        return new PermissionHandler(permissionGrantingService);
    }
}
