using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListTagHelperTests
{
    [Fact]
    public async Task ProcessAsync_Context_CreatesShapeBeforeCollectingColumns()
    {
        string[] items = ["item"];
        var data = new AdminListContext<string> { Source = items };
        var display = new Mock<IDisplayHelper>();
        display.Setup(value => value.ShapeExecuteAsync(It.IsAny<IShape>())).Returns((IShape value) =>
        {
            Assert.Equal(AdminListConstants.ShapeType, value.Metadata.Type);
            Assert.Equal("Items", value.GetProperty<string>("Name"));
            Assert.Same(items, value.GetProperty<System.Collections.IEnumerable>("Source"));
            Assert.Equal(typeof(string), value.GetProperty<Type>("ItemType"));
            Assert.Single(value.GetProperty<AdminListColumnCollection>("Columns"));
            Assert.Equal("rows", value.GetProperty<IDictionary<string, string>>("RowsAttributes")["id"]);
            return Task.FromResult<IHtmlContent>(HtmlString.Empty);
        });
        var helper = new AdminListTagHelper(display.Object, new AdminListTestShapeFactory())
        {
            Context = data,
            Name = "Items",
            RowsAttributes = new Dictionary<string, string> { ["id"] = "rows" },
        };
        var context = CreateContext("admin-list");
        var output = CreateOutput("admin-list", () =>
        {
            RenderColumn(context.Items, new() { Name = "Name", Title = "Name" });
            return Task.CompletedTask;
        });

        await helper.ProcessAsync(context, output);

        display.Verify(value => value.ShapeExecuteAsync(It.IsAny<IShape>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ProcessAsync_InvalidName_Throws(string name)
    {
        var factory = new Mock<IShapeFactory>();
        var display = new Mock<IDisplayHelper>();
        var helper = new AdminListTagHelper(display.Object, factory.Object)
        {
            Name = name,
            Context = new AdminListContext<string> { Source = [] },
        };

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            helper.ProcessAsync(CreateContext("admin-list"), CreateOutput("admin-list")));

        factory.VerifyNoOtherCalls();
        display.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_NullContext_Throws()
    {
        var factory = new Mock<IShapeFactory>();
        var display = new Mock<IDisplayHelper>();
        var helper = new AdminListTagHelper(display.Object, factory.Object) { Name = "Items" };

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            helper.ProcessAsync(CreateContext("admin-list"), CreateOutput("admin-list")));

        factory.VerifyNoOtherCalls();
        display.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessAsync_Children_ConfigureShapeBeforeDisplay()
    {
        var data = new AdminListContext
        {
            ShowLayoutSelector = false,
            Columns = [new() { Name = "Existing" }],
        };
        var context = CreateContext("admin-list");
        var display = new Mock<IDisplayHelper>();
        display.Setup(helper => helper.ShapeExecuteAsync(It.IsAny<IShape>())).Returns((IShape value) =>
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
            Assert.Null(value.GetProperty("Layout"));
            return Task.FromResult<IHtmlContent>(new HtmlString("<div>Rendered shape</div>"));
        });

        var helper = CreateHelper(display.Object, data);
        helper.ShowToolbar = false;
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
        display.Verify(value => value.ShapeExecuteAsync(It.IsAny<IShape>()), Times.Once);
    }

    [Theory]
    [InlineData("admin-list-list")]
    [InlineData("admin-list-grid")]
    public async Task ProcessAsync_DuplicateLayout_Throws(string tag)
    {
        var context = CreateContext("admin-list");
        var helper = CreateHelper();
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
        var options = new AdminListLayoutOptions { ItemCssClass = "existing-row", RowsCssClass = "existing-rows" };
        var context = CreateContext("admin-list");
        var display = new Mock<IDisplayHelper>();
        display.Setup(helper => helper.ShapeExecuteAsync(It.IsAny<IShape>())).ReturnsAsync(HtmlString.Empty);
        var helper = CreateHelper(display.Object, new AdminListContext { GridOptions = options });
        var output = CreateOutput("admin-list", () => RenderLayoutAsync("admin-list-grid", context.Items, null, null));

        await helper.ProcessAsync(context, output);

        Assert.Same(options, ((IShape)context.Items[typeof(AdminListTagHelper)]).Properties["GridOptions"]);
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
        var helper = CreateHelper();
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
        var helper = CreateHelper();
        var output = CreateOutput("admin-list", () =>
        {
            RenderColumn(context.Items, new() { Name = "Name" });
            RenderColumn(context.Items, new() { Name = "name" });
            return Task.CompletedTask;
        });

        await Assert.ThrowsAsync<ArgumentException>(() => helper.ProcessAsync(context, output));
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
            SiteContext.HttpContextAccessor.HttpContext.SetEndpoint(
                new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Admin list rendering"));
            var factory = scope.ServiceProvider.GetRequiredService<IShapeFactory>();
            var listContext = new AdminListContext<ContentItem>
            {
                Source = [new ContentItem { ContentType = "BlogPost", DisplayText = "Item" }],
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
            var list = (IShape)context.Items[typeof(AdminListTagHelper)];
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

    private static AdminListTagHelper CreateHelper(IDisplayHelper display = null, AdminListContext data = null)
        => new(display ?? Mock.Of<IDisplayHelper>(), new AdminListTestShapeFactory())
        {
            Name = "Items",
            Context = data ?? new AdminListContext(),
        };

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
