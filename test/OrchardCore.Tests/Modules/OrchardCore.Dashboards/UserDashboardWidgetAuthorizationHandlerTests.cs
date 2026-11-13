using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Dashboards.Security;
using OrchardCore.Dashboards.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using static OrchardCore.Tests.Modules.OrchardCore.Dashboards.DashboardTestHelper;

namespace OrchardCore.Tests.Modules.OrchardCore.Dashboards;

public class UserDashboardWidgetAuthorizationHandlerTests
{
    [Theory]
    [InlineData("EditContent")]
    [InlineData("DeleteContent")]
    [InlineData("PublishContent")]
    [InlineData("ViewContent")]
    public async Task Handle_OwnWidgetOnUserDashboard_Succeeds(string permissionName)
    {
        var context = await HandleAsync(GetPermission(permissionName), CreateUser("user1", ManageOwn), CreateWidget(UserType, "user1"));

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_OtherUsersWidget_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user2", ManageOwn), CreateWidget(UserType, "user1"));

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_OwnWidgetWithoutManagePermission_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user1"), CreateWidget(UserType, "user1"));

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_SiteDashboardWidget_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user1", ManageOwn, ManageSite), CreateWidget(SiteType, null));

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_PermissionNotForWidgets_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContentOwner, CreateUser("user1", ManageOwn), CreateWidget(UserType, "user1"));

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_NewWidgetWhenUserCanManageOwnDashboard_Succeeds()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user1", ManageOwn), new ContentItem { ContentType = WidgetType });

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_NewWidgetWithoutManagePermission_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user1", ManageSite), new ContentItem { ContentType = WidgetType });

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_NewItemNotAWidget_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user1", ManageOwn), new ContentItem { ContentType = "Article" });

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_NewWidgetDeletePermission_Fails()
    {
        var context = await HandleAsync(CommonPermissions.DeleteContent, CreateUser("user1", ManageOwn), new ContentItem { ContentType = WidgetType });

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task Handle_SavedWidgetWithoutDashboard_Fails()
    {
        var context = await HandleAsync(CommonPermissions.EditContent, CreateUser("user1", ManageOwn), new ContentItem { Id = 3, ContentType = WidgetType });

        Assert.False(context.HasSucceeded);
    }

    private static Permission GetPermission(string name)
        => name switch
        {
            "EditContent" => CommonPermissions.EditContent,
            "DeleteContent" => CommonPermissions.DeleteContent,
            "PublishContent" => CommonPermissions.PublishContent,
            "ViewContent" => CommonPermissions.ViewContent,
            _ => throw new ArgumentOutOfRangeException(nameof(name)),
        };

    private static async Task<AuthorizationHandlerContext> HandleAsync(Permission permission, ClaimsPrincipal user, ContentItem resource)
    {
        var services = new ServiceCollection()
            .AddSingleton<IDashboardManager>(CreateManager())
            .AddSingleton(CreateContentDefinitionManager())
            .BuildServiceProvider();

        var handler = new UserDashboardWidgetAuthorizationHandler(services);
        var context = new AuthorizationHandlerContext([new PermissionRequirement(permission)], user, resource);

        await handler.HandleAsync(context);

        return context;
    }
}
