using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.Admin.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public sealed class AdminListGroupingTests
{
    [Theory]
    [InlineData(3, 3, 5)]
    [InlineData(8, 3, 5)]
    [InlineData(5, 5, 5)]
    [InlineData(1, 1, 1)]
    public void Page_GroupBoundaries_FillsPagesAndRepeatsHeaders(int firstSize, int secondSize, int pageSize)
    {
        var groups = new[]
        {
            new AdminListGroup { Key = "first", Title = "First", Source = Enumerable.Range(0, firstSize).ToArray(), TotalCount = firstSize },
            new AdminListGroup { Key = "second", Title = "Second", Source = Enumerable.Range(firstSize, secondSize).ToArray(), TotalCount = secondSize },
        };
        var seen = new List<int>();
        var count = firstSize + secondSize;
        for (var skip = 0; skip < count; skip += pageSize)
        {
            var page = AdminListGrouping.Page(groups, skip, pageSize);
            var rows = page.SelectMany(group => group.Source.Cast<int>()).ToArray();
            Assert.Equal(Math.Min(pageSize, count - skip), rows.Length);
            foreach (var group in page)
            {
                var original = groups.Single(value => value.Key == group.Key);
                Assert.Equal(original.Title, group.Title);
                Assert.Equal(original.TotalCount, group.TotalCount);
            }

            seen.AddRange(rows);
        }

        Assert.Equal(Enumerable.Range(0, count), seen);
        Assert.Empty(AdminListGrouping.Page(groups, count, pageSize));
        Assert.Empty(AdminListGrouping.Page(groups, 0, 0));
    }

    [Fact]
    public void Selection_UrlOverridesCookie_AndNoneClearsGrouping()
    {
        using var services = new ServiceCollection().AddAdminListGroupingProvider<TestProvider>("Contents").BuildServiceProvider();
        var http = new DefaultHttpContext();
        http.Request.Headers.Cookie = $"{AdminListGroupingService.CookieName}=Contents:Category";
        var accessor = new HttpContextAccessor { HttpContext = http };
        Assert.Equal("Category", new AdminListGroupingService(services, accessor).GetSelection("Contents").Name);

        http.Request.QueryString = new QueryString("?groupBy=None");
        Assert.Null(new AdminListGroupingService(services, accessor).GetSelection("Contents").Provider);
        Assert.Contains("Contents%3ANone", http.Response.Headers.SetCookie.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("?groupBy=missing")]
    [InlineData("?groupBy=..%2Fview")]
    [InlineData("")]
    public void Selection_UnavailableChoice_IsUngroupedAndScopedToList(string query)
    {
        using var services = new ServiceCollection().AddAdminListGroupingProvider<TestProvider>("Contents").BuildServiceProvider();
        var http = new DefaultHttpContext();
        http.Request.QueryString = new QueryString(query);
        var resolver = new AdminListGroupingService(services, new HttpContextAccessor { HttpContext = http });
        Assert.Null(resolver.GetSelection("Contents").Provider);
        Assert.Empty(resolver.GetSelection("Other").Providers);
    }

    [Theory]
    [InlineData("page")]
    [InlineData("pagenum")]
    [InlineData("PageNum")]
    public void GetUrl_NewGrouping_PreservesFiltersAndResetsPage(string pageKey)
    {
        using var services = new ServiceCollection().AddAdminListGroupingProvider<TestProvider>("Contents").BuildServiceProvider();
        var http = new DefaultHttpContext();
        http.Request.PathBase = "/tenant";
        http.Request.Path = "/Admin/Contents/ContentItems";
        http.Request.QueryString = new QueryString($"?q=type%3AArticle&layout=Grid&{pageKey}=4&pageSize=5");
        var resolver = new AdminListGroupingService(services, new HttpContextAccessor { HttpContext = http });
        var url = resolver.GetUrl("Contents", "Category");
        var query = QueryHelpers.ParseQuery(url[url.IndexOf('?')..]);
        Assert.StartsWith("/tenant/Admin/Contents/ContentItems?", url, StringComparison.Ordinal);
        Assert.Equal("type:Article", query["q"]);
        Assert.Equal("Grid", query["layout"]);
        Assert.Equal("5", query["pageSize"]);
        Assert.Equal("Category", query["groupBy"]);
        Assert.False(query.ContainsKey(pageKey));
    }

    [Fact]
    public void Selection_DuplicateIdentifiers_Throws()
    {
        using var services = new ServiceCollection()
            .AddAdminListGroupingProvider<TestProvider>("Contents")
            .AddAdminListGroupingProvider<DuplicateProvider>("Contents")
            .BuildServiceProvider();
        var resolver = new AdminListGroupingService(services, new HttpContextAccessor());
        Assert.Throws<InvalidOperationException>(() => resolver.GetSelection("Contents"));
    }

    [Fact]
    public void Selection_MultipleLists_PreservesEveryPreferenceInTheResponse()
    {
        using var services = new ServiceCollection()
            .AddAdminListGroupingProvider<TestProvider>("First")
            .AddAdminListGroupingProvider<TestProvider>("Second")
            .BuildServiceProvider();
        var http = new DefaultHttpContext();
        http.Request.QueryString = new("?groupBy=Category");
        var resolver = new AdminListGroupingService(services, new HttpContextAccessor { HttpContext = http });
        resolver.GetSelection("First");
        resolver.GetSelection("Second");

        var cookie = http.Response.Headers.SetCookie.ToString();
        Assert.Contains("First%3ACategory", cookie, StringComparison.Ordinal);
        Assert.Contains("Second%3ACategory", cookie, StringComparison.Ordinal);
    }

    private sealed class TestProvider : IAdminListGroupingProvider
    {
        public string Name => "Category";
        public LocalizedString DisplayName => new("Category", "Category");
        public Task<IReadOnlyList<AdminListGroup>> GroupAsync(IEnumerable source, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class DuplicateProvider : IAdminListGroupingProvider
    {
        public string Name => "Category";
        public LocalizedString DisplayName => new("Category", "Category");
        public Task<IReadOnlyList<AdminListGroup>> GroupAsync(IEnumerable source, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
