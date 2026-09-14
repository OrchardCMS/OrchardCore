using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.Navigation;

namespace OrchardCore.Tests.Modules.OrchardCore.AdminDashboard;

public class DashboardBreadcrumbProviderTests
{
    [Fact]
    public async Task BuildBreadcrumbAsync_OnAnAdminRequest_AddsTheDashboardToEveryTrail()
    {
        var provider = CreateProvider("Admin", isAdminRequest: true);

        // The provider reacts to a trail it knows nothing about.
        var builder = new BreadcrumbBuilder("ContentsEdit");

        builder.Add("Manage Content");

        await provider.BuildBreadcrumbAsync(builder);

        var item = Assert.Single(builder.Items, item => item.Id == "Dashboard");

        Assert.Equal("Dashboard", item.Text);
        Assert.Equal("~/Admin", item.Url);

        // 'start' sorts before every other position, so the node leads the trail.
        Assert.Equal("start", item.Position);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_WithACustomAdminPrefix_LinksToThatPrefix()
    {
        var provider = CreateProvider("Backend", isAdminRequest: true);

        var builder = new BreadcrumbBuilder("Contents");

        await provider.BuildBreadcrumbAsync(builder);

        Assert.Equal("~/Backend", Assert.Single(builder.Items).Url);
    }

    [Fact]
    public async Task BuildBreadcrumbAsync_OutsideOfTheAdmin_AddsNothing()
    {
        var provider = CreateProvider("Admin", isAdminRequest: false);

        var builder = new BreadcrumbBuilder("Contents");

        await provider.BuildBreadcrumbAsync(builder);

        Assert.Empty(builder.Items);
    }

    private static DashboardBreadcrumbProvider CreateProvider(string adminUrlPrefix, bool isAdminRequest)
    {
        var httpContext = new DefaultHttpContext();

        if (isAdminRequest)
        {
            AdminAttribute.Apply(httpContext);
        }

        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext,
        };

        var adminOptions = Options.Create(new AdminOptions
        {
            AdminUrlPrefix = adminUrlPrefix,
        });

        return new DashboardBreadcrumbProvider(adminOptions, httpContextAccessor, new StubStringLocalizer<DashboardBreadcrumbProvider>());
    }

    private sealed class StubStringLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] => new(name, string.Format(name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}
