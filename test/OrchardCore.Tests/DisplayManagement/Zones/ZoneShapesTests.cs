using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.ViewModels;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Localization;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement.Zones;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
public class ZoneShapesTests
{
    private readonly ShapeTable _defaultShapeTable;
    private readonly IServiceProvider _serviceProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDisplayHelper _displayHelper;
    private readonly ZoneShapes _zoneShapes;

    public ZoneShapesTests()
    {
        _defaultShapeTable = new ShapeTable(
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IShapeFactory, DefaultShapeFactory>();
        services.AddScoped<IDisplayHelper, DisplayHelper>();
        services.AddScoped<IHtmlDisplay, DefaultHtmlDisplay>();
        services.AddScoped<IShapeTableManager, TestShapeTableManager>();
        services.AddScoped<IThemeManager, ThemeManager>();
        services.AddScoped<IExtensionManager, StubExtensionManager>();
        services.AddScoped<IShapeBindingResolver, TestShapeBindingResolver>();
        services.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
        services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));
        services.AddSingleton(_defaultShapeTable);
        services.AddSingleton(new TestShapeBindingsDictionary());
        services.AddWebEncoders();

        _serviceProvider = services.BuildServiceProvider();
        _shapeFactory = _serviceProvider.GetRequiredService<IShapeFactory>();
        _displayHelper = _serviceProvider.GetRequiredService<IDisplayHelper>();
        _zoneShapes = new ZoneShapes();

        // Add shape descriptors for all the shapes used in tests
        AddBasicShapeDescriptors();
    }

    private void AddBasicShapeDescriptors()
    {
        // Add TestShape
        var testShapeDescriptor = new ShapeDescriptor { ShapeType = "TestShape" };
        AddBinding(testShapeDescriptor, "TestShape", ctx =>
        {
            var name = ctx.Value.Properties.TryGetValue("Name", out var nameValue) ? nameValue?.ToString() : "TestShape";
            return Task.FromResult<IHtmlContent>(new HtmlString($"<div>{name}</div>"));
        });
        AddShapeDescriptor(testShapeDescriptor);

        // Add TabContainer
        var tabContainerDescriptor = new ShapeDescriptor { ShapeType = "TabContainer" };
        AddBinding(tabContainerDescriptor, "TabContainer", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var shape = (IShape)ctx.Value;
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("tab-container");

            foreach (var item in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayHelper.ShapeExecuteAsync((IShape)item));
            }

            return tagBuilder;
        });
        AddShapeDescriptor(tabContainerDescriptor);

        // Add Tab
        var tabDescriptor = new ShapeDescriptor { ShapeType = "Tab" };
        AddBinding(tabDescriptor, "Tab", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var shape = (IShape)ctx.Value;
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("tab");

            foreach (var item in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayHelper.ShapeExecuteAsync((IShape)item));
            }

            return tagBuilder;
        });
        AddShapeDescriptor(tabDescriptor);

        // Add CardContainer
        var cardContainerDescriptor = new ShapeDescriptor { ShapeType = "CardContainer" };
        AddBinding(cardContainerDescriptor, "CardContainer", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var shape = (IShape)ctx.Value;
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("card-container");

            foreach (var item in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayHelper.ShapeExecuteAsync((IShape)item));
            }

            return tagBuilder;
        });
        AddShapeDescriptor(cardContainerDescriptor);

        // Add Card
        var cardDescriptor = new ShapeDescriptor { ShapeType = "Card" };
        AddBinding(cardDescriptor, "Card", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var groupingViewModel = (GroupingViewModel)ctx.Value;
            return await _zoneShapes.CardGrouping(displayHelper, groupingViewModel, _shapeFactory);
        });
        AddShapeDescriptor(cardDescriptor);

        // Add CardGrouping
        var cardGroupingDescriptor = new ShapeDescriptor { ShapeType = "CardGrouping" };
        AddBinding(cardGroupingDescriptor, "CardGrouping", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var groupingViewModel = (GroupingViewModel)ctx.Value;
            return await _zoneShapes.CardGrouping(displayHelper, groupingViewModel, _shapeFactory);
        });
        AddShapeDescriptor(cardGroupingDescriptor);

        // Add ColumnContainer
        var columnContainerDescriptor = new ShapeDescriptor { ShapeType = "ColumnContainer" };
        AddBinding(columnContainerDescriptor, "ColumnContainer", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var shape = (IShape)ctx.Value;
            var tagBuilder = new TagBuilder("div");
            tagBuilder.AddCssClass("row");

            foreach (var item in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayHelper.ShapeExecuteAsync((IShape)item));
            }

            return tagBuilder;
        });
        AddShapeDescriptor(columnContainerDescriptor);

        // Add Column
        var columnDescriptor = new ShapeDescriptor { ShapeType = "Column" };
        AddBinding(columnDescriptor, "Column", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var shape = (IShape)ctx.Value;
            var tagBuilder = new TagBuilder("div");

            // Add all classes from the shape including column classes
            foreach (var cssClass in shape.Classes)
            {
                tagBuilder.AddCssClass(cssClass);
            }

            foreach (var item in shape.Items)
            {
                tagBuilder.InnerHtml.AppendHtml(await displayHelper.ShapeExecuteAsync((IShape)item));
            }

            return tagBuilder;
        });
        AddShapeDescriptor(columnDescriptor);

        // Add ColumnGrouping
        var columnGroupingDescriptor = new ShapeDescriptor { ShapeType = "ColumnGrouping" };
        AddBinding(columnGroupingDescriptor, "ColumnGrouping", async ctx =>
        {
            var displayHelper = ctx.ServiceProvider.GetRequiredService<IDisplayHelper>();
            var groupingViewModel = (GroupingViewModel)ctx.Value;
            return await _zoneShapes.ColumnGrouping(displayHelper, groupingViewModel, _shapeFactory);
        });
        AddShapeDescriptor(columnGroupingDescriptor);
    }

    private void AddShapeDescriptor(ShapeDescriptor shapeDescriptor)
    {
        _defaultShapeTable.Descriptors[shapeDescriptor.ShapeType] = shapeDescriptor;
        foreach (var binding in shapeDescriptor.Bindings)
        {
            _defaultShapeTable.Bindings[binding.Key] = binding.Value;
        }
    }

    private static void AddBinding(ShapeDescriptor descriptor, string bindingName, Func<DisplayContext, Task<IHtmlContent>> binding)
    {
        descriptor.Bindings[bindingName] = new ShapeBinding
        {
            BindingName = bindingName,
            BindingAsync = binding,
        };
    }

    #region Zone Shape Tests

    [Fact]
    public async Task Zone_ShouldRenderAllShapes()
    {
        // Arrange
        var shapes = new List<object>
        {
            CreateTestShape("Shape1"),
            CreateTestShape("Shape2"),
            CreateTestShape("Shape3"),
        };

        // Act
        var result = await _zoneShapes.Zone(_displayHelper, shapes);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Contains("Shape1", html);
        Assert.Contains("Shape2", html);
        Assert.Contains("Shape3", html);
    }

    [Fact]
    public async Task Zone_WithEmptyCollection_ShouldReturnEmpty()
    {
        // Arrange
        var shapes = new List<object>();

        // Act
        var result = await _zoneShapes.Zone(_displayHelper, shapes);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Equal(string.Empty, html);
    }

    #endregion

    #region ContentZone Tests - No Grouping

    [Fact]
    public async Task ContentZone_WithoutGrouping_ShouldRenderDirectly()
    {
        // Arrange
        var shape = await _shapeFactory.CreateAsync<Shape>("TestShape");
        shape.Properties["Name"] = "Test Content";

        var zone = new Shape();
        await zone.AddAsync(shape);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.NotEmpty(html);
        Assert.Contains("Test Content", html);
    }

    [Fact]
    public async Task ContentZone_WithEmptyShapes_ShouldReturnEmpty()
    {
        // Arrange
        var zone = new Shape();

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Equal(string.Empty, html);
    }

    #endregion

    #region ContentZone Tests - Tab Grouping

    [Fact]
    public async Task ContentZone_WithMultipleTabs_ShouldCreateTabContainer()
    {
        // Arrange
        var shape1 = CreateShapeWithTabGrouping("Tab1", "1");
        var shape2 = CreateShapeWithTabGrouping("Tab2", "2");
        var shape3 = CreateShapeWithTabGrouping("Tab1", "3");

        var zone = new Shape();
        zone.Properties["Identifier"] = "test-id";
        await zone.AddAsync(shape1);
        await zone.AddAsync(shape2);
        await zone.AddAsync(shape3);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        // Multiple tabs should create a container
        var html = GetHtmlString(result);
        Assert.Contains("tab-container", html);
        Assert.Contains("Tab1", html);
        Assert.Contains("Tab2", html);
    }

    [Fact]
    public async Task ContentZone_WithSingleTab_ShouldRenderDirectlyWithoutTabContainer()
    {
        // Arrange
        var shape1 = CreateShapeWithTabGrouping("Tab1", "1");
        var shape2 = CreateShapeWithTabGrouping("Tab1", "2");

        var zone = new Shape();
        zone.Properties["Identifier"] = "test-id";
        await zone.AddAsync(shape1);
        await zone.AddAsync(shape2);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);

        // Should NOT create a tab container since there's only one tab
        Assert.DoesNotContain("tab-container", html);

        // Should NOT contain tab-specific markup
        Assert.DoesNotContain("class=\"tab\"", html);

        // Should render the shapes directly (proceeding to card grouping)
        Assert.Contains("Shape in Tab1", html);
    }

    [Fact]
    public async Task ContentZone_TabsWithPosition_ShouldOrderCorrectly()
    {
        // Arrange
        var shape1 = CreateShapeWithTabGrouping("Tab3", "5");
        var shape2 = CreateShapeWithTabGrouping("Tab1", "1");
        var shape3 = CreateShapeWithTabGrouping("Tab2", "2");

        var zone = new Shape();
        zone.Properties["Identifier"] = "test-id";
        await zone.AddAsync(shape1);
        await zone.AddAsync(shape2);
        await zone.AddAsync(shape3);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);

        // Verify the tabs are rendered in order based on position
        var tab1Index = html.IndexOf("Shape in Tab1", StringComparison.Ordinal);
        var tab2Index = html.IndexOf("Shape in Tab2", StringComparison.Ordinal);
        var tab3Index = html.IndexOf("Shape in Tab3", StringComparison.Ordinal);

        Assert.True(tab1Index >= 0, "Tab1 should be present in the output");
        Assert.True(tab2Index >= 0, "Tab2 should be present in the output");
        Assert.True(tab3Index >= 0, "Tab3 should be present in the output");
        Assert.True(tab1Index < tab2Index, "Tab1 should appear before Tab2");
        Assert.True(tab2Index < tab3Index, "Tab2 should appear before Tab3");
    }

    [Fact]
    public async Task ContentZone_TabsWithContentKey_ShouldHandleDefault()
    {
        // Arrange
        var shape1 = CreateShapeWithTabGrouping("Tab1", "1");
        var shape2 = CreateShapeWithoutGrouping(); // This should go to "Content" key

        var zone = new Shape();
        zone.Properties["Identifier"] = "test-id";
        await zone.AddAsync(shape1);
        await zone.AddAsync(shape2);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);

        // Should create a tab container because we have multiple tabs (Content + Tab1)
        Assert.Contains("tab-container", html);

        // Should contain two tab divs
        var tabCount = System.Text.RegularExpressions.Regex.Count(html, "<div class=\"tab\">");
        Assert.Equal(2, tabCount);

        // Should render the shape with explicit Tab1 grouping
        Assert.Contains("Shape in Tab1", html);

        // Should render the shape without grouping (it goes to default "Content" tab)
        Assert.Contains("TestShape", html);
    }

    #endregion

    #region CardGrouping Tests

    [Fact]
    public async Task CardGrouping_WithMultipleCards_ShouldCreateCardContainer()
    {
        // Arrange
        var shape1 = CreateShapeWithCardGrouping("Card1", "1");
        var shape2 = CreateShapeWithCardGrouping("Card2", "2");
        var shape3 = CreateShapeWithCardGrouping("Card1", "3");

        var grouping = CreateMockGrouping(shape1, shape2, shape3);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.CardGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Multiple cards should create a container
        Assert.Contains("card-container", html);
        Assert.Contains("<div>TestShape</div>", html);
    }

    [Fact]
    public async Task CardGrouping_WithSingleCard_ShouldProceedToColumnGrouping()
    {
        // Arrange
        var shape1 = CreateShapeWithCardGrouping("Card1", "1");
        var shape2 = CreateShapeWithCardGrouping("Card1", "2");

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.CardGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Single card should proceed to column grouping and render shapes directly without card container
        Assert.DoesNotContain("card-container", html);
        Assert.Contains("<div>TestShape</div>", html);
    }

    [Fact]
    public async Task CardGrouping_CardsWithPosition_ShouldOrderCorrectly()
    {
        // Arrange
        var shape1 = CreateShapeWithCardGrouping("Card3", "3");
        shape1.Properties["Name"] = "Shape in Card3";
        var shape2 = CreateShapeWithCardGrouping("Card1", "1");
        shape2.Properties["Name"] = "Shape in Card1";
        var shape3 = CreateShapeWithCardGrouping("Card2", "2");
        shape3.Properties["Name"] = "Shape in Card2";

        var grouping = CreateMockGrouping(shape1, shape2, shape3);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.CardGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Cards should be ordered by position: Card1, Card2, Card3
        Assert.Contains("card-container", html);
        Assert.Contains("Shape in Card1", html);
        Assert.Contains("Shape in Card2", html);
        Assert.Contains("Shape in Card3", html);

        // Verify the cards are in order by position
        var card1Index = html.IndexOf("Shape in Card1", StringComparison.Ordinal);
        var card2Index = html.IndexOf("Shape in Card2", StringComparison.Ordinal);
        var card3Index = html.IndexOf("Shape in Card3", StringComparison.Ordinal);

        Assert.True(card1Index >= 0, "Card1 should be present in the output");
        Assert.True(card2Index >= 0, "Card2 should be present in the output");
        Assert.True(card3Index >= 0, "Card3 should be present in the output");
        Assert.True(card1Index < card2Index, "Card1 should appear before Card2");
        Assert.True(card2Index < card3Index, "Card2 should appear before Card3");
    }

    [Fact]
    public async Task CardGrouping_WithContentKey_ShouldHandleDefault()
    {
        // Arrange
        var shape1 = CreateShapeWithCardGrouping("Card1", "1");
        var shape2 = CreateShapeWithoutGrouping(); // Goes to "Content" key

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.CardGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Should handle both grouped and ungrouped shapes
        Assert.Contains("card-container", html);
        Assert.Contains("<div>TestShape</div>", html);
        // Verify there are 2 shapes rendered
        var shapeCount = System.Text.RegularExpressions.Regex.Count(html, "<div>TestShape</div>");
        Assert.Equal(2, shapeCount);
    }

    #endregion

    #region ColumnGrouping Tests

    [Fact]
    public async Task ColumnGrouping_WithMultipleColumns_ShouldCreateColumnContainer()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col1", "1", null);
        var shape2 = CreateShapeWithColumnGrouping("Col2", "2", null);
        var shape3 = CreateShapeWithColumnGrouping("Col1", "3", null);

        var grouping = CreateMockGrouping(shape1, shape2, shape3);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Contains("row", html); // Should contain Bootstrap row class
    }

    [Fact]
    public async Task ColumnGrouping_WithSingleColumn_ShouldRenderDirectly()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col1", "1", null);
        var shape2 = CreateShapeWithColumnGrouping("Col1", "2", null);

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Should render shapes directly without container
        Assert.DoesNotContain("row", html);
        Assert.Contains("<div>TestShape</div>", html);
        // Verify there are 2 shapes rendered
        var shapeCount = System.Text.RegularExpressions.Regex.Count(html, "<div>TestShape</div>");
        Assert.Equal(2, shapeCount);
    }

    [Fact]
    public async Task ColumnGrouping_ColumnsWithPosition_ShouldOrderCorrectly()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col3", "3", null);
        var shape2 = CreateShapeWithColumnGrouping("Col1", "1", null);
        var shape3 = CreateShapeWithColumnGrouping("Col2", "2", null);

        var grouping = CreateMockGrouping(shape1, shape2, shape3);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Columns should be ordered by position: Col1, Col2, Col3
        Assert.Contains("row", html);
        Assert.Contains("ta-col-grouping", html);
        Assert.Contains("column-col1", html);
        Assert.Contains("column-col2", html);
        Assert.Contains("column-col3", html);
        Assert.Contains("col-12 col-md", html);

        // Verify the columns are in order
        var col1Index = html.IndexOf("column-col1", StringComparison.Ordinal);
        var col2Index = html.IndexOf("column-col2", StringComparison.Ordinal);
        var col3Index = html.IndexOf("column-col3", StringComparison.Ordinal);

        Assert.True(col1Index >= 0, "Col1 should be present in the output");
        Assert.True(col2Index >= 0, "Col2 should be present in the output");
        Assert.True(col3Index >= 0, "Col3 should be present in the output");
        Assert.True(col1Index < col2Index, "Col1 should appear before Col2");
        Assert.True(col2Index < col3Index, "Col2 should appear before Col3");
    }

    [Fact]
    public async Task ColumnGrouping_WithWidthModifier_ShouldApplyColumnClasses()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col1", "1", "6");
        var shape2 = CreateShapeWithColumnGrouping("Col2", "2", "6");

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Contains("col-12 col-md-6", html); // Should contain Bootstrap column classes
    }

    [Fact]
    public async Task ColumnGrouping_WithBreakpointModifier_ShouldApplyCorrectClasses()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col1", "1", "lg-4");
        var shape2 = CreateShapeWithColumnGrouping("Col2", "2", "lg-8");

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Contains("col-12 col-lg-4", html);
        Assert.Contains("col-12 col-lg-8", html);
    }

    [Fact]
    public async Task ColumnGrouping_WithoutWidthModifier_ShouldApplyDefaultClasses()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col1", "1", null);
        var shape2 = CreateShapeWithColumnGrouping("Col2", "2", null);

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        Assert.Contains("col-12 col-md", html); // Default classes
    }

    [Fact]
    public async Task ColumnGrouping_WithContentKey_ShouldHandleDefault()
    {
        // Arrange
        var shape1 = CreateShapeWithColumnGrouping("Col1", "1", null);
        var shape2 = CreateShapeWithoutGrouping(); // Goes to "Content" key

        var grouping = CreateMockGrouping(shape1, shape2);
        var viewModel = new GroupingViewModel
        {
            Identifier = "test-id",
            Grouping = grouping,
        };

        // Act
        var result = await _zoneShapes.ColumnGrouping(_displayHelper, viewModel, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Should handle both grouped and ungrouped shapes
        Assert.Contains("row", html);
        Assert.Contains("ta-col-grouping", html);
        Assert.Contains("column-content", html);
        Assert.Contains("column-col1", html);
        Assert.Contains("col-12 col-md", html);
        Assert.Contains("<div>TestShape</div>", html);
    }

    #endregion

    #region Complex Grouping Scenarios

    [Fact]
    public async Task ContentZone_WithTabCardAndColumn_ShouldProcessHierarchically()
    {
        // Arrange - Shapes with Tab -> Card -> Column hierarchy
        var shape1 = CreateShapeWithFullGrouping("Tab1", "1", "Card1", "1", "Col1", "1", "6");
        var shape2 = CreateShapeWithFullGrouping("Tab1", "1", "Card1", "1", "Col2", "2", "6");
        var shape3 = CreateShapeWithFullGrouping("Tab2", "2", "Card2", "1", "Col1", "1", null);

        var zone = new Shape();
        zone.Properties["Identifier"] = "test-id";
        await zone.AddAsync(shape1);
        await zone.AddAsync(shape2);
        await zone.AddAsync(shape3);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Should create tabs first, then cards within tabs, then columns within cards
        Assert.Contains("tab-container", html);
        Assert.Contains("class=\"tab\"", html);
        Assert.Contains("<div>TestShape</div>", html);

        // Verify there are 2 tabs (Tab1 and Tab2)
        var tabCount = System.Text.RegularExpressions.Regex.Count(html, "<div class=\"tab\">");
        Assert.Equal(2, tabCount);

        // Verify there are 3 shapes rendered
        var shapeCount = System.Text.RegularExpressions.Regex.Count(html, "<div>TestShape</div>");
        Assert.Equal(3, shapeCount);
    }

    [Fact]
    public async Task ContentZone_MixedGroupingLevels_ShouldHandleCorrectly()
    {
        // Arrange - Some shapes with tabs, some without
        var shape1 = CreateShapeWithTabGrouping("Tab1", "1");
        var shape2 = CreateShapeWithCardGrouping("Card1", "1"); // No tab, just card
        var shape3 = CreateShapeWithColumnGrouping("Col1", "1", null); // No tab or card

        var zone = new Shape();
        zone.Properties["Identifier"] = "test-id";
        await zone.AddAsync(shape1);
        await zone.AddAsync(shape2);
        await zone.AddAsync(shape3);

        // Act
        dynamic dynamicZone = zone;
        var result = await _zoneShapes.ContentZone(_displayHelper, dynamicZone, _shapeFactory);

        // Assert
        Assert.NotNull(result);
        var html = GetHtmlString(result);
        // Should handle mixed grouping levels gracefully
        Assert.Contains("tab-container", html);
        Assert.Contains("class=\"tab\"", html);

        // Should render the shape with explicit Tab1 grouping
        Assert.Contains("Shape in Tab1", html);

        // Should render all shapes (ungrouped shapes go to default "Content" tab)
        Assert.Contains("<div>TestShape</div>", html);

        // Verify there are 2 tabs (Content and Tab1)
        var tabCount = System.Text.RegularExpressions.Regex.Count(html, "<div class=\"tab\">");
        Assert.Equal(2, tabCount);
    }

    #endregion

    #region Helper Methods

    private Shape CreateTestShape(string name)
    {
        var shape = new Shape
        {
            Metadata =
            {
                Type = "TestShape",
                Name = name,
            },
        };
        shape.Properties["Name"] = name;
        return shape;
    }

    private Shape CreateShapeWithoutGrouping()
    {
        return new Shape
        {
            Metadata = { Type = "TestShape" },
        };
    }

    private Shape CreateShapeWithTabGrouping(string tabName, string position)
    {
        var shape = new Shape
        {
            Metadata = { Type = "TestShape" },
        };
        shape.Metadata.Tab = string.IsNullOrEmpty(position) ? tabName : $"{tabName};{position}";
        shape.Properties["Name"] = $"Shape in {tabName}";
        return shape;
    }

    private Shape CreateShapeWithCardGrouping(string cardName, string position)
    {
        var shape = new Shape
        {
            Metadata = { Type = "TestShape" },
        };
        shape.Metadata.Card = string.IsNullOrEmpty(position) ? cardName : $"{cardName};{position}";
        return shape;
    }

    private Shape CreateShapeWithColumnGrouping(string columnName, string position, string width)
    {
        var shape = new Shape
        {
            Metadata = { Type = "TestShape" },
        };
        shape.Metadata.Column = string.IsNullOrEmpty(position) && string.IsNullOrEmpty(width)
            ? columnName
            : string.IsNullOrEmpty(width)
                ? $"{columnName};{position}"
                : string.IsNullOrEmpty(position)
                    ? $"{columnName}_{width}"
                    : $"{columnName}_{width};{position}";

        return shape;
    }

    private Shape CreateShapeWithFullGrouping(
        string tabName, string tabPosition,
        string cardName, string cardPosition,
        string columnName, string columnPosition, string columnWidth)
    {
        var shape = new Shape
        {
            Metadata = { Type = "TestShape" },
        };

        shape.Metadata.Tab = string.IsNullOrEmpty(tabPosition) ? tabName : $"{tabName};{tabPosition}";
        shape.Metadata.Card = string.IsNullOrEmpty(cardPosition) ? cardName : $"{cardName};{cardPosition}";
        shape.Metadata.Column = string.IsNullOrEmpty(columnPosition) && string.IsNullOrEmpty(columnWidth)
            ? columnName
            : string.IsNullOrEmpty(columnWidth)
                ? $"{columnName};{columnPosition}"
                : string.IsNullOrEmpty(columnPosition)
                    ? $"{columnName}_{columnWidth}"
                    : $"{columnName}_{columnWidth};{columnPosition}";

        return shape;
    }

    private static IGrouping<string, object> CreateMockGrouping(params object[] items)
    {
        return items.ToLookup(x => "TestGroup").First();
    }

    private static string GetHtmlString(IHtmlContent htmlContent)
    {
        using var writer = new StringWriter();
        htmlContent.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
        return writer.ToString();
    }

    #endregion

    #region Test Helper Classes

    private sealed class TestShapeTableManager : IShapeTableManager
    {
        private readonly ShapeTable _shapeTable;

        public TestShapeTableManager(ShapeTable shapeTable)
        {
            _shapeTable = shapeTable;
        }

        public Task<ShapeTable> GetShapeTableAsync(string themeName)
        {
            return Task.FromResult(_shapeTable);
        }
    }

    #endregion
}
