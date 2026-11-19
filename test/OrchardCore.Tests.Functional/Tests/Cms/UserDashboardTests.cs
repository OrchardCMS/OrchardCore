using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;

namespace OrchardCore.Tests.Functional.Tests.Cms;

// Covers the per-user "My Dashboard" of the OrchardCore.Dashboards.User feature, enabled by the
// WidgetDragTests recipe next to the admin dashboard and its two seeded widgets: a widget added to
// My Dashboard is on it, and not on the admin dashboard.
public sealed class UserDashboardTests : CmsTestBase<WidgetDragTestsFixture>, IClassFixture<WidgetDragTestsFixture>
{
    public UserDashboardTests(WidgetDragTestsFixture fixture) : base(fixture) { }

    [Fact]
    public async Task AddWidget_OnMyDashboard_ShowsOnlyOnMyDashboard()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        await page.GotoAndAssertOkAsync("/Admin/dashboards/MyDashboard/manage");

        // The Add Widget menu creates the widget for this dashboard.
        await page.Locator("#new-dropdown").ClickAsync();
        await page.Locator(".add-list-widget").Filter(new LocatorFilterOptions { HasText = "Html Dashboard Widget" }).ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        Assert.Contains("dashboardType=MyDashboard", page.Url);

        await page.Locator("#TitlePart_Title").FillAsync("My Own Widget");
        await page.Locator(".btn.publish").ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Back on the manage page of My Dashboard, through the return URL.
        Assert.Contains("/Admin/dashboards/MyDashboard/manage", page.Url);
        await page.Locator(".dashboard-wrapper").Filter(new LocatorFilterOptions { HasText = "My Own Widget" }).WaitForAsync();

        await page.GotoAndAssertOkAsync("/Admin/dashboards/MyDashboard");
        var myWidgets = await page.Locator(".dashboard-wrapper").AllTextContentsAsync();
        Assert.Contains(myWidgets, text => text.Contains("My Own Widget"));
        Assert.DoesNotContain(myWidgets, text => text.Contains("Dashboard Widget One"));

        await page.GotoAndAssertOkAsync("/Admin");
        var adminWidgets = await page.Locator(".dashboard-wrapper").AllTextContentsAsync();
        Assert.Contains(adminWidgets, text => text.Contains("Dashboard Widget One"));
        Assert.DoesNotContain(adminWidgets, text => text.Contains("My Own Widget"));

        await page.CloseAsync();
    }

    [Fact]
    public async Task Manage_UnknownDashboardType_ReturnsNotFound()
    {
        var page = await Fixture.CreatePageAsync();
        await page.LoginAsync();

        var response = await page.GotoAsync("/Admin/dashboards/Unknown/manage");

        Assert.Equal(404, response?.Status);

        await page.CloseAsync();
    }
}
