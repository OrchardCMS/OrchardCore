using AngleSharp.Html.Parser;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Tests.Apis.Context;
using Arguments = OrchardCore.DisplayManagement.Arguments;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListDisplayTypeTests
{
    [Theory]
    [InlineData("SummaryAdmin", "Content")]
    [InlineData("SummaryAdminGrid", "Name")]
    public async Task ApplyAsync_SharedShape_UsesDisplaySpecificZone(string displayType, string expectedZone)
    {
        var row = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        var child = new Shape();
        var context = new BuildDisplayContext(row, displayType, "", null, null, null);

        await new ShapeResult("Name", _ => ValueTask.FromResult<IShape>(child))
            .Location("SummaryAdmin", "Content:10")
            .Location("SummaryAdminGrid", "Name:20")
            .ApplyAsync(context);

        Assert.Same(child, Assert.Single(row.Zones[expectedZone].Items));
        Assert.Equal(displayType, child.Metadata.DisplayType);
        Assert.False(child.Metadata.ColumnGrouping.HasValue);
        Assert.False(row.Zones.IsNotEmpty(expectedZone == "Name" ? "Content" : "Name"));
    }

    [Theory]
    [InlineData("SummaryAdmin", false)]
    [InlineData("SummaryAdminGrid", true)]
    public async Task ApplyAsync_GridOnlyShape_IsNotBuiltForList(string displayType, bool expected)
    {
        var row = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        var builds = 0;
        var context = new BuildDisplayContext(row, displayType, "", null, null, null);

        await new ShapeResult("Statistics", _ =>
            {
                builds++;
                return ValueTask.FromResult<IShape>(new Shape());
            })
            .Location("SummaryAdminGrid", "Statistics:1")
            .ApplyAsync(context);

        Assert.Equal(expected ? 1 : 0, builds);
        Assert.Equal(expected, row.Zones.IsNotEmpty("Statistics"));
    }

    [Theory]
    [InlineData("Culture:10", "Culture")]
    [InlineData("-", null)]
    public async Task DisplayAsync_GridPlacement_MovesOrHidesShapeWithoutCellCopies(string location, string column)
    {
        using var site = new SiteContext();
        await site.InitializeAsync();

        await site.UsingTenantScopeAsync(async scope =>
        {
            var factory = scope.ServiceProvider.GetRequiredService<IShapeFactory>();
            IShape child = null;
            IZoneHolding row = null;
            var builds = 0;
            async Task<IList<IShape>> BuildRowsAsync(string displayType)
            {
                builds++;
                Assert.Equal("SummaryAdminGrid", displayType);
                row = new ZoneHolding(() => factory.CreateAsync("ContentZone"));
                var context = new BuildDisplayContext(row, displayType, "", factory, null, null)
                {
                    FindPlacement = (_, _, type, _) => type == "SummaryAdminGrid" ? new PlacementInfo(location) : null,
                };
                await new ShapeResult("AdminListTestCell", _ =>
                    {
                        child = new Shape();
                        child.Metadata.ChildContent = new HtmlString("<span data-test-cell=\"moved\">Moved action</span>");
                        return ValueTask.FromResult(child);
                    })
                    .Location("SummaryAdmin", "Actions:10")
                    .Location("SummaryAdminGrid", "Actions:10")
                    .ApplyAsync(context);

                return [row];
            }

            var list = await factory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
            {
                Name = "PlacementTest",
                BuildRowsAsync = (Func<string, Task<IList<IShape>>>)BuildRowsAsync,
                Columns = new AdminListColumn[]
                {
                    new() { Name = "Culture" },
                    AdminListColumns.Actions(new("Actions", "Actions")),
                },
                Layout = AdminListLayout.Grid,
                ShowLayoutSelector = false,
                ShowToolbar = false,
            }));
            Assert.Equal(0, builds);
            var html = await scope.ServiceProvider.GetRequiredService<IDisplayHelper>().ShapeExecuteAsync(list);
            using var writer = new StringWriter();
            html.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            using var document = new HtmlParser().ParseDocument(writer.ToString());

            Assert.Equal(1, builds);
            Assert.False(list.Properties.ContainsKey("Cells"));
            Assert.Empty(document.QuerySelectorAll(".admin-list-column-actions [data-test-cell='moved']"));
            Assert.Empty(document.QuerySelectorAll(".column-container, .column"));
            if (column != null)
            {
                Assert.Single(document.QuerySelectorAll(".admin-list-column-culture [data-test-cell='moved']"));
                Assert.Same(child, Assert.Single(row.Zones[column].Items));
            }
            else
            {
                Assert.Null(child);
                Assert.Empty(document.QuerySelectorAll("[data-test-cell='moved']"));
            }
        });
    }
}
