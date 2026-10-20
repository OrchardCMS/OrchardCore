using OrchardCore.Admin;
using OrchardCore.Admin.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListLayoutResolverTests
{
    [Theory]
    [InlineData(AdminListLayout.List)]
    [InlineData(AdminListLayout.Grid)]
    public void GetLayout_SelectionDisabled_IgnoresQueryAndCookie(AdminListLayout defaultLayout)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?layout=Grid");
        context.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:Grid";
        var resolver = CreateResolver(new AdminListOptions { DefaultLayout = defaultLayout }, context);

        Assert.Equal(defaultLayout, resolver.GetLayout("Contents"));
        Assert.False(resolver.AllowUserSelection);
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Theory]
    [InlineData("List", AdminListLayout.List)]
    [InlineData("grid", AdminListLayout.Grid)]
    [InlineData(" Grid ", AdminListLayout.Grid)]
    public void GetLayout_QueryOverridesCookie_IsCanonicalAndRemembered(string query, AdminListLayout expected)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = QueryString.Create("layout", query);
        context.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:List";
        var resolver = CreateResolver(new AdminListOptions { AllowUserSelection = true }, context);

        Assert.Equal(expected, resolver.GetLayout("Contents"));
        if (expected == AdminListLayout.Grid)
        {
            Assert.Contains("Contents%3AGrid", context.Response.Headers.SetCookie.ToString());
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Cards")]
    [InlineData("1")]
    [InlineData("List, Grid")]
    [InlineData("Contents")]
    public void GetLayout_InvalidQuery_UsesStoredPreference(string query)
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = QueryString.Create("layout", query);
        context.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:Grid|Users:List";
        var resolver = CreateResolver(new AdminListOptions { AllowUserSelection = true }, context);

        Assert.Equal(AdminListLayout.Grid, resolver.GetLayout("Contents"));
        Assert.Equal(AdminListLayout.List, resolver.GetLayout("Users"));
        Assert.False(context.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Theory]
    [InlineData("Cards")]
    [InlineData("1")]
    [InlineData("List,Grid")]
    public void GetLayout_InvalidCookie_UsesSiteDefault(string preference)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{AdminListLayoutPreference.CookieName}=Contents:{preference}";
        var resolver = CreateResolver(new AdminListOptions
        {
            DefaultLayout = AdminListLayout.Grid,
            AllowUserSelection = true,
        }, context);

        Assert.Equal(AdminListLayout.Grid, resolver.GetLayout("Contents"));
        Assert.Equal(AdminListLayout.Grid, resolver.GetLayout("Queries"));
    }

    [Fact]
    public void GetLayout_NoHttpContext_UsesDefaultAndDisablesSelector()
    {
        var resolver = CreateResolver(new AdminListOptions
        {
            DefaultLayout = AdminListLayout.Grid,
            AllowUserSelection = true,
        }, null);

        Assert.Equal(AdminListLayout.Grid, resolver.GetLayout("Contents"));
        Assert.False(resolver.AllowUserSelection);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void GetLayout_InvalidListName_Throws(string name)
    {
        var resolver = CreateResolver(new AdminListOptions(), new DefaultHttpContext());

        Assert.ThrowsAny<ArgumentException>(() => resolver.GetLayout(name));
    }

    [Theory]
    [InlineData(AdminListLayout.List)]
    [InlineData(AdminListLayout.Grid)]
    public void GetLayoutUrl_KeepsPathBaseFiltersAndPage_ReplacesLayout(AdminListLayout layout)
    {
        var context = new DefaultHttpContext();
        context.Request.PathBase = "/tenant";
        context.Request.Path = "/Admin/Contents/ContentItems";
        context.Request.QueryString = new QueryString("?q=post&page=2&pageSize=10&Layout=Grid");
        var resolver = CreateResolver(new AdminListOptions { AllowUserSelection = true }, context);

        Assert.Equal($"/tenant/Admin/Contents/ContentItems?q=post&page=2&pageSize=10&layout={layout}", resolver.GetLayoutUrl(layout));
    }

    [Fact]
    public void GetLayoutUrl_UndefinedLayout_Throws()
    {
        var resolver = CreateResolver(new AdminListOptions(), new DefaultHttpContext());

        Assert.Throws<ArgumentOutOfRangeException>(() => resolver.GetLayoutUrl((AdminListLayout)42));
    }

    private static AdminListLayoutResolver CreateResolver(AdminListOptions options, HttpContext context)
    {
        var accessor = new HttpContextAccessor { HttpContext = context };
        return new AdminListLayoutResolver(
            Mock.Of<IOptionsMonitor<AdminListOptions>>(monitor => monitor.CurrentValue == options),
            accessor,
            new AdminListLayoutPreference(accessor));
    }
}
