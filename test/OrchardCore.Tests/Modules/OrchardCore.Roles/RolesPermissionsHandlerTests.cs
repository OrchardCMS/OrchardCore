using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using OrchardCore.Roles;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Security;
using Xunit;

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
            var roleManager = RolesMockHelper.MockRoleManager<IRole>();

            var anonymousRole = new Role
            {
                RoleName = "Anonymous",
                RoleClaims = new List<RoleClaim> {
                    new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "AllowAnonymous" }
                }
            };
            roleManager.Setup(m => m.FindByNameAsync(anonymousRole.RoleName)).ReturnsAsync(anonymousRole);

            var authenticatedRole = new Role
            {
                RoleName = "Authenticated",
                RoleClaims = new List<RoleClaim> {
                    new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "AllowAuthenticated" }
                }
            };
            roleManager.Setup(m => m.FindByNameAsync(authenticatedRole.RoleName)).ReturnsAsync(authenticatedRole);

            var permissionHandler = new RolesPermissionsHandler(roleManager.Object);

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
            var roleManager = RolesMockHelper.MockRoleManager<IRole>();
            var permissionHandler = new RolesPermissionsHandler(roleManager.Object);

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
            var roleManager = RolesMockHelper.MockRoleManager<IRole>();

            var anonymousRole = new Role
            {
                RoleName = "Anonymous",
                RoleClaims = new List<RoleClaim> {
                    new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "Implicit2" }
                }
            };
            roleManager.Setup(m => m.FindByNameAsync(anonymousRole.RoleName)).ReturnsAsync(anonymousRole);

            var permissionHandler = new RolesPermissionsHandler(roleManager.Object);

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
            var roleManager = RolesMockHelper.MockRoleManager<IRole>();

            var anonymousRole = new Role
            {
                RoleName = "Anonymous",
                RoleClaims = new List<RoleClaim> {
                    new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "aLlOwAnOnYmOuS" }
                }
            };
            roleManager.Setup(m => m.FindByNameAsync(anonymousRole.RoleName)).ReturnsAsync(anonymousRole);

            var authenticatedRole = new Role
            {
                RoleName = "Authenticated",
                RoleClaims = new List<RoleClaim> {
                    new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = "aLlOwAuThEnTiCaTeD" }
                }
            };
            roleManager.Setup(m => m.FindByNameAsync(authenticatedRole.RoleName)).ReturnsAsync(authenticatedRole);

            var permissionHandler = new RolesPermissionsHandler(roleManager.Object);

            // Act
            await permissionHandler.HandleAsync(context);

            // Assert
            Assert.True(context.HasSucceeded);
        }
    }
}
