using OrchardCore.Navigation;

namespace OrchardCore.Tests.Navigation;

public class PageSizeSelectorTests
{
    [Fact]
    public void BuildOptions_SelectionAllowed_KeepsTheQueryStringAndResetsThePaging()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.PathBase = "/tenant";
        httpContext.Request.Path = "/Admin/Contents/ContentItems";
        httpContext.Request.QueryString = new QueryString("?q=post&pagenum=3&PageSize=10&before=a&after=b&layout=Grid");

        var items = PageSizeSelector.BuildOptions(CreateServices(httpContext, allowSelection: true), currentPageSize: 25);

        Assert.Collection(items,
            item =>
            {
                Assert.Equal("10", item.Text);
                Assert.Equal("/tenant/Admin/Contents/ContentItems?q=post&layout=Grid&pageSize=10", item.Value);
                Assert.False(item.Selected);
            },
            item =>
            {
                Assert.Equal("25", item.Text);
                Assert.Equal("/tenant/Admin/Contents/ContentItems?q=post&layout=Grid&pageSize=25", item.Value);
                Assert.True(item.Selected);
            });
    }

    [Fact]
    public void BuildOptions_SelectionTurnedOff_ReturnsNull()
    {
        var items = PageSizeSelector.BuildOptions(CreateServices(new DefaultHttpContext(), allowSelection: false), currentPageSize: 10);

        Assert.Null(items);
    }

    private static ServiceProvider CreateServices(HttpContext httpContext, bool allowSelection)
        => new ServiceCollection()
            .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext })
            .AddSingleton(Options.Create(new PagerOptions
            {
                AllowPageSizeSelection = allowSelection,
                PageSizeOptions = [10, 25],
            }))
            .BuildServiceProvider();
}
