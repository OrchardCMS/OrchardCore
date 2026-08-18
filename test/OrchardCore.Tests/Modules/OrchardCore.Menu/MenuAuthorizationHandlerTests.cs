using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Menu.Services;
using OrchardCore.Roles;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Tests.Security;

namespace OrchardCore.Tests.Modules.OrchardCore.Menu;

public class MenuAuthorizationHandlerTests
{
    [Fact]
    public async Task MenuContentItem_WithoutMenuPermission_FailsForGenericViewRequirement()
    {
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.ViewContent,
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.True(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentItem_WithDirectMenuPermissionClaim_SucceedsForGenericViewRequirement()
    {
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var viewMenuPermission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ViewContent.Name],
            global::OrchardCore.Menu.MenuConstants.MenuContentType);
        var claims = new[]
        {
            new Claim(Permission.ClaimType, viewMenuPermission.Name),
        };
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.ViewContent,
            claims,
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task OwnedMenuContentItem_WithDirectOwnMenuPermissionClaim_SucceedsForGenericViewRequirement()
    {
        const string userId = "user-id";

        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var viewOwnMenuPermission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ViewOwnContent.Name],
            global::OrchardCore.Menu.MenuConstants.MenuContentType);
        var claims = new[]
        {
            new Claim(Permission.ClaimType, viewOwnMenuPermission.Name),
            new Claim(ClaimTypes.NameIdentifier, userId),
        };
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.ViewContent,
            claims,
            authenticated: true,
            resource: new ContentItem
            {
                ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType,
                Owner = userId,
            });

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentType_WithManageMenu_GrantsListAuthorization()
    {
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var listMenuPermission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ListContent.Name],
            global::OrchardCore.Menu.MenuConstants.MenuContentType);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            listMenuPermission,
            [global::OrchardCore.Menu.Permissions.ManageMenu.Name],
            authenticated: true,
            resource: global::OrchardCore.Menu.MenuConstants.MenuContentType);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentItem_WithManageMenu_GrantsEditAuthorization()
    {
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.EditContent,
            [global::OrchardCore.Menu.Permissions.ManageMenu.Name],
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentItem_WithAdministratorRole_DoesNotFailAuthorization()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Role, OrchardCoreConstants.Roles.Administrator),
        };
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.EditContent,
            claims,
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.False(context.HasFailed);
    }

    [Fact]
    public async Task MenuContentItem_WithEditMenuPermission_DoesNotGrantDeleteAuthorization()
    {
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var editMenuPermission = ContentTypePermissionsHelper.CreateDynamicPermission(
            ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditContent.Name],
            global::OrchardCore.Menu.MenuConstants.MenuContentType);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            CommonPermissions.DeleteContent,
            [editMenuPermission.Name],
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
    }

    [Theory]
    [MemberData(nameof(ManageMenuDeniedPermissions))]
    public async Task MenuContentItem_WithManageMenu_DoesNotGrantUnrelatedAuthorization(Permission permission)
    {
        using var serviceProvider = CreateServiceProvider();
        var handler = CreateHandler(serviceProvider);
        var context = PermissionHandlerHelper.CreateTestAuthorizationHandlerContext(
            permission,
            [global::OrchardCore.Menu.Permissions.ManageMenu.Name],
            authenticated: true,
            resource: new ContentItem { ContentType = global::OrchardCore.Menu.MenuConstants.MenuContentType });

        await handler.HandleAsync(context);

        Assert.False(context.HasSucceeded);
        Assert.True(context.HasFailed);
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

    public static TheoryData<Permission> ManageMenuDeniedPermissions()
        => new()
        {
            CommonPermissions.DeleteContent,
            CommonPermissions.PublishContent,
            CommonPermissions.EditContentOwner,
        };

    private static ServiceProvider CreateServiceProvider(bool isSuperUser = false)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizationService>(new TestAuthorizationService(isSuperUser));
        services.AddSingleton<ISystemRoleProvider>(new TestSystemRoleProvider());
        services.AddSingleton<ISiteService>(new TestSiteService(isSuperUser ? "user-id" : null));

        return services.BuildServiceProvider();
    }

    private static MenuAuthorizationHandler CreateHandler(ServiceProvider serviceProvider)
        => new(
            serviceProvider,
            serviceProvider.GetRequiredService<ISystemRoleProvider>(),
            serviceProvider.GetRequiredService<ISiteService>());

    private sealed class TestSystemRoleProvider : ISystemRoleProvider
    {
        private readonly Role _adminRole = new()
        {
            RoleName = OrchardCoreConstants.Roles.Administrator,
        };

        public IRole GetAdminRole() => _adminRole;

        public IEnumerable<IRole> GetSystemRoles() => [_adminRole];

        public bool IsSystemRole(string name) => string.Equals(name, _adminRole.RoleName, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestSiteService : ISiteService
    {
        private readonly ISite _site;

        public TestSiteService(string superUser)
        {
            _site = new TestSite
            {
                SuperUser = superUser,
            };
        }

        public Task<ISite> GetSiteSettingsAsync() => Task.FromResult(_site);

        public Task<ISite> LoadSiteSettingsAsync() => Task.FromResult(_site);

        public Task UpdateSiteSettingsAsync(ISite site) => Task.CompletedTask;
    }

    private sealed class TestSite : ISite
    {
        public JsonObject Properties { get; } = [];

        public string SiteName { get; set; }

        public string PageTitleFormat { get; set; }

        public string BaseUrl { get; set; }

        public string TimeZoneId { get; set; }

        public string Calendar { get; set; }

        public ResourceDebugMode ResourceDebugMode { get; set; }

        public bool UseCdn { get; set; }

        public string CdnBaseUrl { get; set; }

        public string SuperUser { get; set; }

        public int PageSize { get; set; }

        public int MaxPageSize { get; set; }

        public int MaxPagedCount { get; set; }

        public string SiteSalt { get; set; }

        public RouteValueDictionary HomeRoute { get; set; }

        public bool AppendVersion { get; set; }

        public CacheMode CacheMode { get; set; }

        [Obsolete]
        public T As<T>() where T : new() => new();

        public T GetOrCreate<T>() where T : new() => new();

        public bool TryGet<T>(out T settings)
        {
            settings = default;

            return false;
        }
    }

    private sealed class TestAuthorizationService : IAuthorizationService
    {
        private readonly bool _isSuperUser;
        private readonly DefaultPermissionGrantingService _permissionGrantingService = new();

        public TestAuthorizationService(bool isSuperUser)
        {
            _isSuperUser = isSuperUser;
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
        {
            if (_isSuperUser)
            {
                return Task.FromResult(AuthorizationResult.Success());
            }

            var requirementsArray = requirements.OfType<PermissionRequirement>().ToArray();

            if (requirementsArray.All(requirement =>
                user.HasClaim(Permission.ClaimType, requirement.Permission.Name)
                || _permissionGrantingService.IsGranted(requirement, user.Claims)))
            {
                return Task.FromResult(AuthorizationResult.Success());
            }

            return Task.FromResult(AuthorizationResult.Failed());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
            => Task.FromResult(AuthorizationResult.Failed());
    }
}
