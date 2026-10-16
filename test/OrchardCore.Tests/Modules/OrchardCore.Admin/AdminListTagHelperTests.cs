using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListTagHelperTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ProcessAsync_ItemsOrContext_CreatesShapeBeforeCollectingColumns(bool useContext)
    {
        var shape = CreateShape();
        string[] items = ["item"];
        var data = new AdminListContext<string> { Source = items };
        var factory = new Mock<IAdminListFactory>();
        factory.Setup(value => value.CreateAsync("Items", It.IsAny<AdminListContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, AdminListContext value, CancellationToken _) =>
            {
                Assert.Equal("Items", name);
                if (useContext)
                {
                    Assert.Same(data, value);
                    shape.Properties["Source"] = value.Source;
                }

                return shape;
            });
        var display = new Mock<IDisplayHelper>();
        display.Setup(value => value.ShapeExecuteAsync(shape)).Returns((IShape value) =>
        {
            Assert.Same(items, value.GetProperty<System.Collections.IEnumerable>("Source"));
            Assert.Single(value.GetProperty<AdminListColumnCollection>("Columns"));
            Assert.Equal("rows", value.GetProperty<IDictionary<string, string>>("RowsAttributes")["id"]);
            return Task.FromResult<IHtmlContent>(HtmlString.Empty);
        });
        var helper = new AdminListTagHelper(display.Object, factory.Object)
        {
            Context = useContext ? data : null,
            Name = "Items",
            Items = useContext ? null : items,
            RowsAttributes = new Dictionary<string, string> { ["id"] = "rows" },
        };
        var context = CreateContext("admin-list");
        var output = CreateOutput("admin-list", () =>
        {
            RenderColumn(context.Items, new() { Name = "Name", Title = "Name" });
            return Task.CompletedTask;
        });

        await helper.ProcessAsync(context, output);

        factory.Verify(value => value.CreateAsync("Items", It.IsAny<AdminListContext>(), It.IsAny<CancellationToken>()), Times.Once);
        display.Verify(value => value.ShapeExecuteAsync(shape), Times.Once);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, "")]
    [InlineData(false, " ")]
    [InlineData(true, null)]
    [InlineData(true, "")]
    [InlineData(true, " ")]
    public async Task ProcessAsync_InvalidNameWithItemsOrContext_Throws(bool useContext, string name)
    {
        var factory = new Mock<IAdminListFactory>();
        var display = new Mock<IDisplayHelper>();
        var helper = new AdminListTagHelper(display.Object, factory.Object)
        {
            Name = name,
            Context = useContext ? new AdminListContext<string> { Source = [] } : null,
            Items = useContext ? null : Array.Empty<string>(),
        };

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            helper.ProcessAsync(CreateContext("admin-list"), CreateOutput("admin-list")));

        factory.VerifyNoOtherCalls();
        display.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(null, "Existing")]
    [InlineData("FromView", "FromView")]
    public async Task ProcessAsync_ExistingShape_PreservesOrOverridesName(string name, string expected)
    {
        var shape = CreateShape();
        shape.Properties["Name"] = "Existing";
        var factory = new Mock<IAdminListFactory>();
        var display = new Mock<IDisplayHelper>();
        display.Setup(value => value.ShapeExecuteAsync(shape)).Returns((IShape value) =>
        {
            Assert.Equal(expected, value.GetProperty<string>("Name"));
            return Task.FromResult<IHtmlContent>(HtmlString.Empty);
        });
        var helper = new AdminListTagHelper(display.Object, factory.Object) { Shape = shape, Name = name };

        await helper.ProcessAsync(CreateContext("admin-list"), CreateOutput("admin-list"));

        factory.VerifyNoOtherCalls();
        display.Verify(value => value.ShapeExecuteAsync(shape), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ProcessAsync_ExistingShapeWithInvalidNameOverride_Throws(string name)
    {
        var shape = CreateShape();
        shape.Properties["Name"] = "Existing";
        var helper = new AdminListTagHelper(Mock.Of<IDisplayHelper>(), Mock.Of<IAdminListFactory>())
        {
            Shape = shape,
            Name = name,
        };

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            helper.ProcessAsync(CreateContext("admin-list"), CreateOutput("admin-list")));

        Assert.Equal("Existing", shape.GetProperty<string>("Name"));
    }

    [Fact]
    public async Task ProcessAsync_Children_ConfigureShapeBeforeDisplay()
    {
        var shape = CreateShape();
        shape.Properties["ShowLayoutSelector"] = false;
        shape.Properties["Columns"] = new AdminListColumn[] { new() { Name = "Existing" } };
        var context = CreateContext("admin-list");
        var display = new Mock<IDisplayHelper>();
        display.Setup(helper => helper.ShapeExecuteAsync(shape)).Returns((IShape value) =>
        {
            Assert.Equal(false, value.Properties["ShowToolbar"]);
            Assert.Equal(false, value.Properties["ShowLayoutSelector"]);
            Assert.Equal("list-row", value.GetProperty<AdminListLayoutOptions>("ListOptions").ItemCssClass);
            Assert.Equal("grid-row", value.GetProperty<AdminListLayoutOptions>("GridOptions").ItemCssClass);
            var columns = value.GetProperty<AdminListColumnCollection>("Columns");
            Assert.Equal(["Existing", "Select", "Name", "Actions"], columns.Select(column => column.Name));
            Assert.Equal("Name & <value>", columns["Name"].Title.Value);
            Assert.Equal("10", columns["Select"].Position);
            Assert.Equal("end", columns["Actions"].Position);
            Assert.Equal(["Actions", "ActionsMenu"], columns["Actions"].Zones);
            Assert.False(value.Properties.ContainsKey("Layout"));
            return Task.FromResult<IHtmlContent>(new HtmlString("<div>Rendered shape</div>"));
        });

        var helper = new AdminListTagHelper(display.Object, Mock.Of<IAdminListFactory>()) { Shape = shape, ShowToolbar = false };
        var output = CreateOutput("admin-list", async () =>
        {
            await RenderLayoutAsync("admin-list-list", context.Items, "list-row", "list-rows");
            await RenderLayoutAsync("admin-list-grid", context.Items, "grid-row", "grid-rows", () =>
            {
                RenderColumn(context.Items, new() { Type = "Select" });
                RenderColumn(context.Items, new() { Name = "Name", Title = new LocalizedHtmlString("Name {0}", "Name {0}", false, "& <value>"), Position = "20" });
                RenderColumn(context.Items, new() { Type = "Actions", Title = "Actions" });
                return Task.CompletedTask;
            });
        });

        await helper.ProcessAsync(context, output);

        Assert.Null(output.TagName);
        Assert.Empty(output.Attributes);
        Assert.Equal("<div>Rendered shape</div>", output.Content.GetContent());
        display.Verify(value => value.ShapeExecuteAsync(shape), Times.Once);
    }

    [Theory]
    [InlineData("admin-list-list")]
    [InlineData("admin-list-grid")]
    public async Task ProcessAsync_DuplicateLayout_Throws(string tag)
    {
        var context = CreateContext("admin-list");
        var helper = new AdminListTagHelper(Mock.Of<IDisplayHelper>(), Mock.Of<IAdminListFactory>()) { Shape = CreateShape() };
        var output = CreateOutput("admin-list", async () =>
        {
            await RenderLayoutAsync(tag, context.Items, null, null);
            await RenderLayoutAsync(tag, context.Items, null, null);
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => helper.ProcessAsync(context, output));
    }

    [Fact]
    public async Task ProcessAsync_OmittedLayoutAttributes_PreserveExistingSettings()
    {
        var shape = CreateShape();
        var options = new AdminListLayoutOptions { ItemCssClass = "existing-row", RowsCssClass = "existing-rows" };
        shape.Properties["GridOptions"] = options;
        var context = CreateContext("admin-list");
        var display = new Mock<IDisplayHelper>();
        display.Setup(helper => helper.ShapeExecuteAsync(shape)).ReturnsAsync(HtmlString.Empty);
        var helper = new AdminListTagHelper(display.Object, Mock.Of<IAdminListFactory>()) { Shape = shape };
        var output = CreateOutput("admin-list", () => RenderLayoutAsync("admin-list-grid", context.Items, null, null));

        await helper.ProcessAsync(context, output);

        Assert.Same(options, shape.Properties["GridOptions"]);
        Assert.Equal("existing-row", options.ItemCssClass);
        Assert.Equal("existing-rows", options.RowsCssClass);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, " ")]
    [InlineData("Unknown", "Name")]
    public async Task ProcessAsync_InvalidColumn_Throws(string type, string name)
    {
        var context = CreateContext("admin-list");
        var helper = new AdminListTagHelper(Mock.Of<IDisplayHelper>(), Mock.Of<IAdminListFactory>()) { Shape = CreateShape() };
        var output = CreateOutput("admin-list", () =>
        {
            RenderColumn(context.Items, new() { Type = type, Name = name });
            return Task.CompletedTask;
        });

        if (type == null)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => helper.ProcessAsync(context, output));
        }
        else
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => helper.ProcessAsync(context, output));
        }
    }

    [Fact]
    public async Task ProcessAsync_DuplicateColumns_Throws()
    {
        var context = CreateContext("admin-list");
        var helper = new AdminListTagHelper(Mock.Of<IDisplayHelper>(), Mock.Of<IAdminListFactory>()) { Shape = CreateShape() };
        var output = CreateOutput("admin-list", () =>
        {
            RenderColumn(context.Items, new() { Name = "Name" });
            RenderColumn(context.Items, new() { Name = "name" });
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<ArgumentException>(() => helper.ProcessAsync(context, output));
    }

    [Theory]
    [InlineData("Other", false)]
    [InlineData("AdminList", true)]
    public async Task ProcessAsync_InvalidShape_Throws(string type, bool initialized)
    {
        var shape = CreateShape();
        shape.Metadata.Type = type;
        shape.Properties["AdminListInitialized"] = initialized;
        var helper = new AdminListTagHelper(Mock.Of<IDisplayHelper>(), Mock.Of<IAdminListFactory>()) { Shape = shape };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            helper.ProcessAsync(CreateContext("admin-list"), CreateOutput("admin-list")));
    }

    [Fact]
    public void Process_ColumnOverrides_PreservesPresetDefaultsAndAppliesAttributes()
    {
        var columns = new AdminListColumnCollection();
        var context = CreateContext("admin-list-column");
        context.Items[typeof(AdminListColumnTagHelper)] = columns;
        var helper = new AdminListColumnTagHelper
        {
            Type = "Actions",
            Name = "Custom",
            Position = "35",
            Width = "12rem",
            Alignment = AdminListColumnAlignment.Center,
            Zones = "Primary, Secondary Other",
            CssClass = "custom-column",
            NoWrap = true,
        };
        var output = CreateOutput("admin-list-column");

        helper.Process(context, output);

        var column = Assert.Single(columns);
        Assert.Equal("Custom", column.Name);
        Assert.Equal("35", column.Position);
        Assert.Equal("12rem", column.Width);
        Assert.Equal(AdminListColumnAlignment.Center, column.Alignment);
        Assert.Equal(["Primary", "Secondary", "Other"], column.Zones);
        Assert.Equal("custom-column", column.CssClass);
        Assert.True(column.NoWrap);
        Assert.True(output.IsContentModified);
        Assert.Null(output.TagName);
    }

    [Theory]
    [InlineData(AdminListLayout.List)]
    [InlineData(AdminListLayout.Grid)]
    public async Task ProcessAsync_LayoutSettings_RenderOnlyInSelectedLayout(AdminListLayout layout)
    {
        using var site = new SiteContext();
        await site.InitializeAsync();
        await site.UsingTenantScopeAsync(async scope =>
        {
            var factory = scope.ServiceProvider.GetRequiredService<IAdminListFactory>();
            var row = await scope.ServiceProvider.GetRequiredService<IShapeFactory>().CreateAsync("ContentZone");
            var listContext = new AdminListContext
            {
                Rows = [row],
                Layout = layout,
                ShowToolbar = false,
                ShowLayoutSelector = false,
                RowsAttributes = new Dictionary<string, string> { ["class"] = "existing-rows", ["data-test"] = "<value>" },
                ItemCssClass = "legacy-list-row",
            };
            var context = CreateContext("admin-list");
            var helper = new AdminListTagHelper(scope.ServiceProvider.GetRequiredService<IDisplayHelper>(), factory)
            {
                Name = "TagHelperTest",
                Context = listContext,
            };
            var output = CreateOutput("admin-list", async () =>
            {
                await RenderLayoutAsync("admin-list-list", context.Items, "only-list-row", "only-list-rows");
                await RenderLayoutAsync("admin-list-grid", context.Items, "only-grid-row", "only-grid-rows", () =>
                {
                    RenderColumn(context.Items, new() { Name = "Name", Title = new LocalizedHtmlString("Name {0}", "Name {0}", false, "& <value>") });
                    return Task.CompletedTask;
                });
            });

            await helper.ProcessAsync(context, output);
            var list = helper.Shape;
            var html = output.Content.GetContent();
            var selected = layout.ToString().ToLowerInvariant();
            var other = layout == AdminListLayout.Grid ? "list" : "grid";
            using var document = new HtmlParser().ParseDocument(html);
            Assert.Equal(layout, list.GetProperty<AdminListLayout>("Layout"));
            Assert.Single(document.QuerySelectorAll($".only-{selected}-row"));
            Assert.Single(document.QuerySelectorAll($".only-{selected}-rows.existing-rows"));
            Assert.Contains($"only-{selected}-rows existing-rows", html);
            Assert.DoesNotContain($"only-{other}-row", html);
            Assert.DoesNotContain("legacy-list-row", html);
            Assert.Contains("data-test=\"&lt;value&gt;\"", html);
            Assert.DoesNotContain("<admin-list", html);
            if (layout == AdminListLayout.Grid)
            {
                Assert.Equal("Name & <value>", Assert.Single(document.QuerySelectorAll("[role='columnheader']")).TextContent.Trim());
                Assert.Equal("Name & <value>", Assert.Single(document.QuerySelectorAll("[role='cell']")).GetAttribute("data-title"));
            }
        });
    }

    private static Shape CreateShape()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.ShapeType;
        return shape;
    }

    private static TagHelperContext CreateContext(string tag, IDictionary<object, object> items = null)
        => new(tag, [], items == null ? new Dictionary<object, object>() : new Dictionary<object, object>(items), Guid.NewGuid().ToString("N"));

    private static TagHelperOutput CreateOutput(string tag, Func<Task> children = null)
        => new(tag, [], async (_, _) =>
        {
            if (children != null)
            {
                await children();
            }

            return new DefaultTagHelperContent();
        });

    private static async Task RenderLayoutAsync(string tag, IDictionary<object, object> items, string itemClass, string rowsClass, Func<Task> children = null)
    {
        var helper = new AdminListLayoutTagHelper { ItemCssClass = itemClass, RowsClass = rowsClass };
        var output = CreateOutput(tag, children);
        await helper.ProcessAsync(CreateContext(tag, items), output);
        Assert.Null(output.TagName);
    }

    private static void RenderColumn(IDictionary<object, object> items, AdminListColumnTagHelper helper)
        => helper.Process(CreateContext("admin-list-column", items), CreateOutput("admin-list-column"));
}
