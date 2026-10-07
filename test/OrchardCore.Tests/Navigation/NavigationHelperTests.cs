using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Navigation;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Navigation;

public class NavigationHelperTests
{
    #region Test factories

    private static ViewContext CreateViewContext(string requestPath, string pathBase = "")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = requestPath;
        httpContext.Request.PathBase = pathBase;
        return new ViewContext { HttpContext = httpContext };
    }

    private static MenuItem CreateMenuItem(string href, string text = "Item") =>
        new()
        {
            Href = href,
            Text = new LocalizedString(text, text),
        };

    private static NavigationItemViewModel CreateItemShape(bool selected, int score = 0, int priority = 0) =>
        new() { Selected = selected, Score = score, Priority = priority };

    private static void InvokeMarkAsSelectedIfMatchesPath(
        MenuItem menuItem, NavigationItemViewModel shape, ViewContext viewContext) =>
        NavigationHelper.MarkAsSelectedIfMatchesPath(menuItem, shape, viewContext);

    private static void InvokeApplySelection(IShape parentShape) =>
        NavigationHelper.ApplySelection(parentShape);

    #endregion

    // -----------------------------------------------------------------------
    // UseLegacyFormat
    // -----------------------------------------------------------------------

    [Fact]
    public void UseLegacyFormat_WhenSwitchDisabled_ReturnsFalse()
    {
        AppContext.SetSwitch(NavigationConstants.LegacyAdminMenuNavigationSwitchKey, false);
        Assert.False(NavigationHelper.UseLegacyFormat());
    }

    [Fact]
    public void UseLegacyFormat_WhenSwitchEnabled_ReturnsTrue()
    {
        AppContext.SetSwitch(NavigationConstants.LegacyAdminMenuNavigationSwitchKey, true);
        try
        {
            Assert.True(NavigationHelper.UseLegacyFormat());
        }
        finally
        {
            AppContext.SetSwitch(NavigationConstants.LegacyAdminMenuNavigationSwitchKey, false);
        }
    }

    // -----------------------------------------------------------------------
    // CountPathSegments
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("/", 0)]
    [InlineData("/Admin", 1)]
    [InlineData("/Admin/", 1)]
    [InlineData("/Admin/Contents", 2)]
    [InlineData("/Admin/Contents/", 2)]
    [InlineData("/a/b/c", 3)]
    public void CountPathSegments_ReturnsCorrectCount(string path, int expected)
    {
        var result = NavigationHelper.CountPathSegments(path);
        Assert.Equal(expected, result);
    }

    // -----------------------------------------------------------------------
    // CountLeadingMatchingPathSegments  (also covers CollapseSlashes indirectly)
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("/Admin", "/Admin", 1)]
    [InlineData("/Admin/Contents", "/Admin/Contents", 2)]
    [InlineData("/Admin/Contents/123", "/Admin/Contents", 2)]   // request longer — prefix match
    [InlineData("/Admin", "/Admin/Contents", 1)]                // request shorter — partial match only
    [InlineData("/Other", "/Admin", 0)]                         // no match
    [InlineData("", "/Admin", 0)]                               // empty request
    [InlineData("/Admin", "", 0)]                               // empty href
    [InlineData("/ADMIN/CONTENTS", "/admin/contents", 2)]       // case-insensitive
    public void CountLeadingMatchingPathSegments_ReturnsCorrectCount(
        string requestPath, string hrefPath, int expected)
    {
        var result = NavigationHelper.CountLeadingMatchingPathSegments(requestPath, hrefPath);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CountLeadingMatchingPathSegments_WithDoubleSlashInRequest_MatchesNormalized()
    {
        // Fix #5 — embedded double-slashes must be collapsed before comparison.
        var result = NavigationHelper.CountLeadingMatchingPathSegments("/Admin//Contents", "/Admin/Contents");

        Assert.Equal(2, result);
    }

    [Fact]
    public void CountLeadingMatchingPathSegments_WithDoubleSlashInHref_MatchesNormalized()
    {
        var result = NavigationHelper.CountLeadingMatchingPathSegments("/Admin/Contents", "/Admin//Contents");

        Assert.Equal(2, result);
    }

    [Fact]
    public void CountLeadingMatchingPathSegments_MismatchAfterMatchingSegments_ReturnsMatchCount()
    {
        var result = NavigationHelper.CountLeadingMatchingPathSegments("/Admin/Other", "/Admin/Contents");

        Assert.Equal(1, result);
    }

    // -----------------------------------------------------------------------
    // RemovePathBase
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("/Admin/Contents", "", "/Admin/Contents")]
    [InlineData("/Admin/Contents", "/", "/Admin/Contents")]
    [InlineData("/myapp/Admin/Contents", "/myapp", "/Admin/Contents")]
    [InlineData("/myapp", "/myapp", "/")]
    [InlineData("/myapp/", "/myapp", "/")]
    [InlineData("/other/path", "/myapp", "/other/path")]
    [InlineData("/myapp/Admin", "/myapp/", "/Admin")]          // trailing slash in pathBase
    public void RemovePathBase_ReturnsCorrectPath(string path, string pathBase, string expected)
    {
        var result = NavigationHelper.RemovePathBase(path, new PathString(pathBase));

        Assert.Equal(expected, result);
    }

    // -----------------------------------------------------------------------
    // ComputeStableHash
    // -----------------------------------------------------------------------

    [Fact]
    public void ComputeStableHash_NullParentHash_ReturnsNonEmptyString()
    {
        var result = NavigationHelper.ComputeStableHash(null, "Content");
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ComputeStableHash_EmptyParentHash_ReturnsNonEmptyString()
    {
        var result = NavigationHelper.ComputeStableHash("", "Content");
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ComputeStableHash_SameInputs_ReturnIdenticalHash()
    {
        var h1 = NavigationHelper.ComputeStableHash(null, "Content");
        var h2 = NavigationHelper.ComputeStableHash(null, "Content");
        Assert.Equal(h1, h2);
    }

    [Fact]
    public void ComputeStableHash_DifferentValues_ReturnDifferentHashes()
    {
        var h1 = NavigationHelper.ComputeStableHash(null, "Content");
        var h2 = NavigationHelper.ComputeStableHash(null, "Other");
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void ComputeStableHash_WithAndWithoutParentHash_ReturnDifferentHashes()
    {
        var withParent = NavigationHelper.ComputeStableHash("parent", "Content");
        var withoutParent = NavigationHelper.ComputeStableHash(null, "Content");
        Assert.NotEqual(withParent, withoutParent);
    }

    [Fact]
    public void ComputeStableHash_DifferentParentHashes_ReturnDifferentHashes()
    {
        var h1 = NavigationHelper.ComputeStableHash("parent1", "Content");
        var h2 = NavigationHelper.ComputeStableHash("parent2", "Content");
        Assert.NotEqual(h1, h2);
    }

    // -----------------------------------------------------------------------
    // MarkAsSelectedIfMatchesPath
    // -----------------------------------------------------------------------

    [Fact]
    public void MarkAsSelectedIfMatchesPath_ExactMatch_SetsSelectedAndPositiveScore()
    {
        var item = CreateMenuItem("/Admin/Contents");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.True(shape.Selected);
        Assert.True(shape.Score > 0);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_SingleSegmentPrefixHref_DoesNotSetSelectedForDeeperRequest()
    {
        var item = CreateMenuItem("/Admin");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.False(shape.Selected);
        Assert.Equal(0, shape.Score);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_NoPathMatch_DoesNotSetSelected()
    {
        var item = CreateMenuItem("/Other");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.False(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_RelativeHref_DoesNotSetSelected()
    {
        var item = CreateMenuItem("relative/path");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/relative/path"));

        Assert.False(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_EmptyHref_DoesNotSetSelected()
    {
        var item = CreateMenuItem("");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin"));

        Assert.False(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_HrefWithQueryString_StripsQueryAndMatches()
    {
        var item = CreateMenuItem("/Admin/Contents?page=1&filter=active");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.True(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_HrefWithFragment_StripsFragmentAndMatches()
    {
        // Fix #2 — fragment must be stripped before path comparison.
        var item = CreateMenuItem("/Admin/Contents#section");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.True(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_HrefWithQueryAndFragment_StripsAllAndMatches()
    {
        var item = CreateMenuItem("/Admin/Contents?page=1#section");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.True(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_FragmentBeforeQueryString_StripsCorrectly()
    {
        // Unusual but valid: href with fragment appearing before a literal '?' in the fragment text.
        // The code should pick the earliest of '#' and '?'.
        var item = CreateMenuItem("/Admin/Contents#frag?param=1");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.True(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_WithPathBase_StripsPathBaseAndMatches()
    {
        var item = CreateMenuItem("/Admin/Contents");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/myapp/Admin/Contents", "/myapp"));

        Assert.True(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_CaseInsensitiveMatch()
    {
        var item = CreateMenuItem("/Admin/Contents");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/admin/contents"));

        Assert.True(shape.Selected);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_ExactMatchScoresHigherThanPrefixMatch()
    {
        var exactItem = CreateMenuItem("/Admin/Contents");
        var prefixItem = CreateMenuItem("/Admin");
        var exactShape = new NavigationItemViewModel();
        var prefixShape = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin/Contents");

        InvokeMarkAsSelectedIfMatchesPath(exactItem, exactShape, viewContext);
        InvokeMarkAsSelectedIfMatchesPath(prefixItem, prefixShape, viewContext);

        Assert.True(exactShape.Score > prefixShape.Score);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_LongerMatchingPrefixScoresHigher()
    {
        // /Admin/Contents should score higher than /Admin when request is /Admin/Contents/123
        var deeper = CreateMenuItem("/Admin/Contents");
        var shallower = CreateMenuItem("/Admin");
        var deeperShape = new NavigationItemViewModel();
        var shallowerShape = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin/Contents/123");

        InvokeMarkAsSelectedIfMatchesPath(deeper, deeperShape, viewContext);
        InvokeMarkAsSelectedIfMatchesPath(shallower, shallowerShape, viewContext);

        Assert.True(deeperShape.Score > shallowerShape.Score);
    }

    [Fact]
    public void MarkAsSelectedIfMatchesPath_SingleSegmentHref_RequestLongerThanHref_DoesNotMatch()
    {
        // /Admin should not match /Admin/Contents because only one leading segment matches.
        var item = CreateMenuItem("/Admin");
        var shape = new NavigationItemViewModel();

        InvokeMarkAsSelectedIfMatchesPath(item, shape, CreateViewContext("/Admin/Contents"));

        Assert.False(shape.Selected);
        Assert.Equal(0, shape.Score);
    }

    // -----------------------------------------------------------------------
    // ApplySelection / GetHighestPrioritySelectedMenuItem
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ApplySelection_SingleSelectedItem_RemainsSelected()
    {
        var root = new NavigationItemViewModel();
        var child = CreateItemShape(selected: true, score: 2);
        await root.AddAsync(child, "");

        InvokeApplySelection(root);

        Assert.True(child.Selected);
    }

    [Fact]
    public async Task ApplySelection_TwoSelectedItems_HigherScoreWins()
    {
        var root = new NavigationItemViewModel();
        var low = CreateItemShape(selected: true, score: 1);
        var high = CreateItemShape(selected: true, score: 3);
        await root.AddAsync(low, "1");
        await root.AddAsync(high, "2");

        InvokeApplySelection(root);

        Assert.False(low.Selected);
        Assert.True(high.Selected);
    }

    [Fact]
    public async Task ApplySelection_EqualScore_HigherPriorityWins()
    {
        var root = new NavigationItemViewModel();
        var lowPriority = CreateItemShape(selected: true, score: 2, priority: 0);
        var highPriority = CreateItemShape(selected: true, score: 2, priority: 10);
        await root.AddAsync(lowPriority, "1");
        await root.AddAsync(highPriority, "2");

        InvokeApplySelection(root);

        Assert.False(lowPriority.Selected);
        Assert.True(highPriority.Selected);
    }

    [Fact]
    public async Task ApplySelection_SelectedLeaf_PropagatesSelectionToParent()
    {
        var root = new NavigationItemViewModel();
        var parent = CreateItemShape(selected: false);
        var child = CreateItemShape(selected: true, score: 2);
        child.Parent = parent;
        parent.Parent = root;
        await root.AddAsync(parent, "1");
        await parent.AddAsync(child, "1");

        InvokeApplySelection(root);

        Assert.True(child.Selected);
        Assert.True(parent.Selected);
    }

    [Fact]
    public async Task ApplySelection_NoSelectedItems_NothingChanges()
    {
        var root = new NavigationItemViewModel();
        var child1 = CreateItemShape(selected: false);
        var child2 = CreateItemShape(selected: false);
        await root.AddAsync(child1, "1");
        await root.AddAsync(child2, "2");

        InvokeApplySelection(root);

        Assert.False(child1.Selected);
        Assert.False(child2.Selected);
    }

    [Fact]
    public async Task ApplySelection_DeepTree_OnlyHighestScoredLeafAndAncestorsSelected()
    {
        var root = new NavigationItemViewModel();

        var branch1 = CreateItemShape(selected: false);
        var leaf1 = CreateItemShape(selected: true, score: 2);    // /Admin match
        leaf1.Parent = branch1;
        branch1.Parent = root;
        await root.AddAsync(branch1, "1");
        await branch1.AddAsync(leaf1, "1");

        var branch2 = CreateItemShape(selected: false);
        var leaf2 = CreateItemShape(selected: true, score: 5);   // /Admin/Contents match — wins
        leaf2.Parent = branch2;
        branch2.Parent = root;
        await root.AddAsync(branch2, "2");
        await branch2.AddAsync(leaf2, "1");

        InvokeApplySelection(root);

        Assert.False(leaf1.Selected);
        Assert.False(branch1.Selected);
        Assert.True(leaf2.Selected);
        Assert.True(branch2.Selected);
    }

    // -----------------------------------------------------------------------
    // PopulateMenuAsync — end-to-end integration
    // -----------------------------------------------------------------------

    private static IShapeFactory BuildShapeFactory()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddScoped<ILoggerFactory, NullLoggerFactory>();
        services.AddScoped<IThemeManager, ThemeManager>();
        services.AddScoped<IShapeFactory, DefaultShapeFactory>();
        services.AddScoped<IExtensionManager, StubExtensionManager>();
        services.AddScoped<IShapeTableManager, TestShapeTableManager>();
        services.AddSingleton(new ShapeTable(
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)));

        return services.BuildServiceProvider().GetRequiredService<IShapeFactory>();
    }

    [Fact]
    public async Task PopulateMenuAsync_SingleItem_CreatesMenuItemShape()
    {
        var shapeFactory = BuildShapeFactory();
        var menu = new NavigationItemViewModel();
        var parent = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin");

        var items = new[]
        {
            CreateMenuItem("/Admin", "Dashboard"),
        };

        await NavigationHelper.PopulateMenuAsync(shapeFactory, parent, menu, items, viewContext);

        Assert.Single(parent.Items);
        var item = parent.Items[0] as NavigationItemViewModel;
        Assert.NotNull(item);
        Assert.Equal("Dashboard", item.Text.Value);
    }

    [Fact]
    public async Task PopulateMenuAsync_ExactUrlMatch_MarksItemSelected()
    {
        var shapeFactory = BuildShapeFactory();
        var menu = new NavigationItemViewModel();
        var parent = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin/Contents");

        var items = new[]
        {
            CreateMenuItem("/Other", "Other"),
            CreateMenuItem("/Admin/Contents", "Contents"),
        };

        await NavigationHelper.PopulateMenuAsync(shapeFactory, parent, menu, items, viewContext);

        var shapes = parent.Items.OfType<NavigationItemViewModel>().ToList();
        Assert.False(shapes[0].Selected);
        Assert.True(shapes[1].Selected);
    }

    [Fact]
    public async Task PopulateMenuAsync_MultipleMatchingItems_OnlyHighestScoredIsSelected()
    {
        var shapeFactory = BuildShapeFactory();
        var menu = new NavigationItemViewModel();
        var parent = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin/Contents");

        // Both match the request path, but /Admin/Contents is a more specific match.
        var items = new[]
        {
            CreateMenuItem("/Admin", "Admin"),
            CreateMenuItem("/Admin/Contents", "Contents"),
        };

        await NavigationHelper.PopulateMenuAsync(shapeFactory, parent, menu, items, viewContext);

        var shapes = parent.Items.OfType<NavigationItemViewModel>().ToList();
        var adminShape = shapes.Single(s => s.Text.Value == "Admin");
        var contentsShape = shapes.Single(s => s.Text.Value == "Contents");

        Assert.True(contentsShape.Selected);
        Assert.False(adminShape.Selected);
    }

    [Fact]
    public async Task PopulateMenuAsync_NestedItems_BuildsHierarchy()
    {
        var shapeFactory = BuildShapeFactory();
        var menu = new NavigationItemViewModel();
        var parent = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin");

        var child = CreateMenuItem("/Admin/Sub", "Sub");
        var root = new MenuItem
        {
            Href = "/Admin",
            Text = new LocalizedString("Admin", "Admin"),
            Items = [child],
        };

        await NavigationHelper.PopulateMenuAsync(shapeFactory, parent, menu, [root], viewContext);

        var rootShape = parent.Items.OfType<NavigationItemViewModel>().Single();
        Assert.Single(rootShape.Items);
        var childShape = rootShape.Items[0] as NavigationItemViewModel;
        Assert.NotNull(childShape);
        Assert.Equal("Sub", childShape.Text.Value);
    }

    [Fact]
    public async Task PopulateMenuAsync_SelectedChildItem_PropagatesSelectionToParent()
    {
        var shapeFactory = BuildShapeFactory();
        var menu = new NavigationItemViewModel();
        var parent = new NavigationItemViewModel();
        var viewContext = CreateViewContext("/Admin/Contents");

        var child = CreateMenuItem("/Admin/Contents", "Contents");
        var rootItem = new MenuItem
        {
            Href = "/Admin",
            Text = new LocalizedString("Admin", "Admin"),
            Items = [child],
        };

        await NavigationHelper.PopulateMenuAsync(shapeFactory, parent, menu, [rootItem], viewContext);

        var rootShape = parent.Items.OfType<NavigationItemViewModel>().Single();
        var childShape = rootShape.Items.OfType<NavigationItemViewModel>().Single();

        Assert.True(childShape.Selected);
        Assert.True(rootShape.Selected);
    }
}
