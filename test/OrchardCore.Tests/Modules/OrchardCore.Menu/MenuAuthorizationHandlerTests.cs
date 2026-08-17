using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Menu.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class MenuAuthorizationHandlerTests
{
    [Fact]
    public async Task MenuContentItem_WithoutManageMenu_FailsWhenNoDirectPermissionClaim()
    {
        using var serviceProvider = CreateServiceProvider(granted: false);
        var handler = new MenuAuthorizationHandler(serviceProvider);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.ViewContent,
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.True(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentItem_WithDirectMenuPermissionClaim_SucceedsWithoutManageMenu()
    {
        using var serviceProvider = CreateServiceProvider(granted: false);
        var handler = new MenuAuthorizationHandler(serviceProvider);
        var viewMenuPermission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ViewContent.Name],
            global::OrchardCore.Menu.MenuConstants.MenuContentType);
        var claims = new[] { new Claim(Permission.ClaimType, viewMenuPermission.Name) };
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            viewMenuPermission,
            claims,
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentType_WithManageMenu_GrantsGenericContentAuthorization()
    {
        using var serviceProvider = CreateServiceProvider(granted: true);
        var handler = new MenuAuthorizationHandler(serviceProvider);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.ListContent,
            authenticated: true,
            resource: global::OrchardCore.Menu.MenuConstants.MenuContentType);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task MenuDynamicPermission_WithManageMenu_GrantsAuthorization()
    {
        using var serviceProvider = CreateServiceProvider(granted: true);
        var handler = new MenuAuthorizationHandler(serviceProvider);
        var dynamicPermission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name],
            global::OrchardCore.Menu.MenuConstants.MenuContentType);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            dynamicPermission,
            authenticated: true,
            resource: global::OrchardCore.Menu.MenuConstants.MenuContentType);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public void ManageMenu_WithEditMenuContentPermission_IsGranted()
    {
        var claims = new[]
        {
            new Claim(Permission.ClaimType, "Edit_Menu"),
        };
        var permissionGrantingService = new DefaultPermissionGrantingService();

        Assert.True(permissionGrantingService.IsGranted(
            new PermissionRequirement(global::OrchardCore.Menu.Permissions.ManageMenu),
            claims));
    }

    [Fact]
    public void ManageMenu_WithEditContentPermission_IsGranted()
    {
        var claims = new[]
        {
            new Claim(Permission.ClaimType, CommonPermissions.EditContent.Name),
        };
        var permissionGrantingService = new DefaultPermissionGrantingService();

        Assert.True(permissionGrantingService.IsGranted(
            new PermissionRequirement(global::OrchardCore.Menu.Permissions.ManageMenu),
            claims));
    }

    [Fact]
    public void ListMenuContent_WithAuthorAndAuthenticatedClaims_IsNotGranted()
    {
        var claims = new[]
        {
            new Claim(Permission.ClaimType, CommonPermissions.PublishOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.EditOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.DeleteOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.PreviewOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.CloneOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.ViewContent.Name),
        };
        var permissionGrantingService = new DefaultPermissionGrantingService();

        Assert.False(permissionGrantingService.IsGranted(
            new PermissionRequirement(new Permission("ListContent_Menu")),
            claims));
    }

    [Fact]
    public void ManageMenu_WithAuthorAndAuthenticatedClaims_IsNotGranted()
    {
        var claims = new[]
        {
            new Claim(Permission.ClaimType, CommonPermissions.PublishOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.EditOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.DeleteOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.PreviewOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.CloneOwnContent.Name),
            new Claim(Permission.ClaimType, CommonPermissions.ViewContent.Name),
        };
        var permissionGrantingService = new DefaultPermissionGrantingService();

        Assert.False(permissionGrantingService.IsGranted(
            new PermissionRequirement(global::OrchardCore.Menu.Permissions.ManageMenu),
            claims));
    }

    private static ServiceProvider CreateServiceProvider(bool granted)
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                    requirements.OfType<PermissionRequirement>().Any(requirement =>
                        requirement.Permission.Name == global::OrchardCore.Menu.Permissions.ManageMenu.Name))))
            .ReturnsAsync(granted ? AuthorizationResult.Success() : AuthorizationResult.Failed());

        var services = new ServiceCollection();
        services.AddSingleton(authorizationService.Object);

        return services.BuildServiceProvider();
    }
}
