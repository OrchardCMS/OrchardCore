using static OrchardCore.Tests.Modules.OrchardCore.Dashboards.DashboardTestHelper;

namespace OrchardCore.Tests.Modules.OrchardCore.Dashboards;

public class DashboardManagerTests
{
    [Fact]
    public void GetType_IgnoresCase_ReturnsRegisteredType()
    {
        var manager = CreateManager();

        Assert.Equal(UserType, manager.GetType(UserType.ToLowerInvariant())?.Name);
        Assert.Null(manager.GetType("Unknown"));
        Assert.Null(manager.GetType(null));
    }

    [Fact]
    public async Task CanViewAsync_SiteDashboardWithViewPermission_Succeeds()
    {
        var manager = CreateManager();
        var type = manager.GetType(SiteType);

        Assert.True(await manager.CanViewAsync(CreateUser("user1", ViewSite), type));
        Assert.False(await manager.CanManageAsync(CreateUser("user1", ViewSite), type));
        Assert.False(await manager.CanViewAsync(CreateUser("user1"), type));
    }

    [Fact]
    public async Task CanViewAsync_ManagePermission_ImpliesView()
    {
        var manager = CreateManager();

        Assert.True(await manager.CanViewAsync(CreateUser("user1", ManageSite), manager.GetType(SiteType)));
        Assert.True(await manager.CanViewAsync(CreateUser("user1", ManageOwn), manager.GetType(UserType)));
    }

    [Fact]
    public async Task CanManageAsync_UserDashboardWithoutUserId_Fails()
    {
        var manager = CreateManager();

        Assert.False(await manager.CanManageAsync(CreateUser(null, ManageOwn), manager.GetType(UserType)));
    }

    [Fact]
    public void IsOnDashboard_UserDashboard_OnlyOwnWidgets()
    {
        var manager = CreateManager();
        var type = manager.GetType(UserType);

        Assert.True(manager.IsOnDashboard(CreateUser("user1"), type, CreateWidget(UserType, "user1")));
        Assert.False(manager.IsOnDashboard(CreateUser("user2"), type, CreateWidget(UserType, "user1")));
    }

    [Fact]
    public void IsOnDashboard_WidgetOfAnotherDashboard_Fails()
    {
        var manager = CreateManager();

        Assert.False(manager.IsOnDashboard(CreateUser("user1"), manager.GetType(SiteType), CreateWidget(UserType, "user1")));
        Assert.False(manager.IsOnDashboard(CreateUser("user1"), manager.GetType(UserType), CreateWidget(SiteType, null)));
        Assert.True(manager.IsOnDashboard(CreateUser("user1"), manager.GetType(SiteType), CreateWidget(SiteType, null)));
    }
}
