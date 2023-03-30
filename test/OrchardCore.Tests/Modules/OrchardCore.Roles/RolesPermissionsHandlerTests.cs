using OrchardCore.Roles;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles
{
    public class RolesPermissionsHandlerTests
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
                    RoleName = "Anonymous",
                    RoleClaims = new List<RoleClaim> {
                        new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "AllowAnonymous" }
                    }
                },
                new Role
                {
                    RoleName = "Authenticated",
                    RoleClaims = new List<RoleClaim> {
                        new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "AllowAuthenticated" }
                    }
                }
            );

            // Act
            await permissionHandler.HandleAsync(context);

            // Assert
            Assert.Equal(success, context.HasSucceeded);
        }

        [Fact]
        public async Task DontRevokeExistingGrants()
        {
            // Arrange
            var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission("Required"), new[] { "Other" }, true);

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
                    RoleName = "Anonymous",
                    RoleClaims = new List<RoleClaim> {
                        new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "Implicit2" }
                    }
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
        public async Task IsCaseIsensitive(string required, bool authenticated)
        {
            // Arrange
            var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(new Permission(required), authenticated: authenticated);

            var permissionHandler = CreatePermissionHandler(
                new Role
                {
                    RoleName = "Anonymous",
                    RoleClaims = new List<RoleClaim> {
                        new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "aLlOwAnOnYmOuS" }
                    }
                },
                new Role
                {
                    RoleName = "Authenticated",
                    RoleClaims = new List<RoleClaim> {
                        new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "aLlOwAuThEnTiCaTeD" }
                    }
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
}
