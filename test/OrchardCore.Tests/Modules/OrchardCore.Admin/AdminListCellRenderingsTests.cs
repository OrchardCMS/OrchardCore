using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListCellRenderingsTests
{
    private const string AdminViews = "Areas/OrchardCore.Admin/Views/";
    private const string ThemeViews = "Areas/TheTheme/Views/";

    [Fact]
    public void Get_OnlyTheShippedTemplates_RendersTheZonesAndTheActionsDirectly()
    {
        var shapeTable = CreateShapeTable(
            ("AdminListCell", AdminViews + "AdminListCell.cshtml"),
            ("AdminListCell__Actions", AdminViews + "AdminListCell-Actions.cshtml"));

        Assert.Equal(AdminListCellRendering.Zones, AdminListCellRenderings.Get(shapeTable, "Contents", "Title"));
        Assert.Equal(AdminListCellRendering.Actions, AdminListCellRenderings.Get(shapeTable, "Contents", "Actions"));
    }

    [Fact]
    public void Get_TemplateOverridingAColumnOfAList_RendersThatColumnOfThatListThroughTheShape()
    {
        var shapeTable = CreateShapeTable(
            ("AdminListCell", AdminViews + "AdminListCell.cshtml"),
            ("AdminListCell__Contents__Title", ThemeViews + "AdminListCell-Contents-Title.cshtml"));

        Assert.Equal(AdminListCellRendering.Shape, AdminListCellRenderings.Get(shapeTable, "Contents", "Title"));
        Assert.Equal(AdminListCellRendering.Zones, AdminListCellRenderings.Get(shapeTable, "Contents", "Type"));
        Assert.Equal(AdminListCellRendering.Zones, AdminListCellRenderings.Get(shapeTable, "Users", "Title"));
    }

    [Fact]
    public void Get_TemplateOverridingTheActionsCell_RendersTheActionsThroughTheShape()
    {
        var shapeTable = CreateShapeTable(
            ("AdminListCell", AdminViews + "AdminListCell.cshtml"),
            ("AdminListCell__Actions", ThemeViews + "AdminListCell-Actions.liquid"));

        Assert.Equal(AdminListCellRendering.Shape, AdminListCellRenderings.Get(shapeTable, "Contents", "Actions"));
    }

    [Fact]
    public void Get_TemplateOrShapeOverridingEveryCell_RendersEveryColumnThroughTheShape()
    {
        var themeOverride = CreateShapeTable(("AdminListCell", ThemeViews + "AdminListCell.cshtml"));

        // A shape declared in code is bound to the name of the method declaring it, not to a template of this module.
        var codeOverride = CreateShapeTable(("AdminListCell", "AdminListCell"));

        foreach (var shapeTable in new[] { themeOverride, codeOverride })
        {
            Assert.Equal(AdminListCellRendering.Shape, AdminListCellRenderings.Get(shapeTable, "Contents", "Title"));
            Assert.Equal(AdminListCellRendering.Shape, AdminListCellRenderings.Get(shapeTable, "Contents", "Actions"));
        }
    }

    [Fact]
    public async Task Get_TheShapeTableOfASite_RecognizesTheTemplatesOfTheAdminModule()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var shapeTable = await scope.ServiceProvider.GetRequiredService<IShapeTableManager>().GetShapeTableAsync("TheAdmin");

            // The binding sources of the templates are what tells them from an override, so they are checked for real.
            Assert.True(AdminListCellRenderings.IsShipped(shapeTable.Bindings["AdminListCell"]), shapeTable.Bindings["AdminListCell"].BindingSource);
            Assert.True(AdminListCellRenderings.IsShipped(shapeTable.Bindings["AdminListCell__Actions"]), shapeTable.Bindings["AdminListCell__Actions"].BindingSource);

            Assert.Equal(AdminListCellRendering.Zones, AdminListCellRenderings.Get(shapeTable, "Users", "User"));
            Assert.Equal(AdminListCellRendering.Actions, AdminListCellRenderings.Get(shapeTable, "Users", "Actions"));
        });
    }

    private static ShapeTable CreateShapeTable(params (string ShapeType, string Source)[] bindings)
        => new(
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            bindings.ToDictionary(
                binding => binding.ShapeType.ToLowerInvariant(),
                binding => new ShapeBinding { BindingName = binding.ShapeType.ToLowerInvariant(), BindingSource = binding.Source },
                StringComparer.OrdinalIgnoreCase));
}
