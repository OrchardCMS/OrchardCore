using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListShapeTableProviderTests
{
    [Fact]
    public async Task AdminList_AddsLayoutAndNameAlternates_MostSpecificLast()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListLayouts.ShapeType;
        shape.Properties["Name"] = "Contents";
        shape.Properties["Layout"] = AdminListLayouts.Table;

        await DisplayAsync(AdminListLayouts.ShapeType, shape);

        Assert.Equal(
            ["AdminList__Table", "AdminList__Contents", "AdminList__Contents__Table"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminList_DefaultsToListLayout_WhenLayoutIsMissing()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListLayouts.ShapeType;

        await DisplayAsync(AdminListLayouts.ShapeType, shape);

        Assert.Equal(["AdminList__List"], shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListActions_AddsLayoutAndListAlternates()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListActionsLayouts.ShapeType;
        shape.Properties["ListName"] = "Contents";
        shape.Properties["Layout"] = AdminListActionsLayouts.Menu;

        await DisplayAsync(AdminListActionsLayouts.ShapeType, shape);

        Assert.Equal(
            ["AdminListActions__Menu", "AdminListActions__Contents", "AdminListActions__Contents__Menu"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListActions_DefaultsToButtons_WithoutLayoutOrServices()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListActionsLayouts.ShapeType;

        await DisplayAsync(AdminListActionsLayouts.ShapeType, shape);

        Assert.Equal(AdminListActionsLayouts.Buttons, shape.Properties["Layout"]);
        Assert.Equal(["AdminListActions__Buttons"], shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListCell_AddsColumnAndListAlternates()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListLayouts.CellShapeType;
        shape.Properties["ListName"] = "Contents";
        shape.Properties["Column"] = new AdminListColumn { Name = "Actions" };

        await DisplayAsync(AdminListLayouts.CellShapeType, shape);

        Assert.Equal(
            ["AdminListCell__Actions", "AdminListCell__Contents__Actions"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListCell_AddsNoAlternates_WithoutColumn()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListLayouts.CellShapeType;

        await DisplayAsync(AdminListLayouts.CellShapeType, shape);

        Assert.Empty(shape.Metadata.Alternates);
    }

    private static async Task DisplayAsync(string shapeType, Shape shape)
    {
        var feature = new Mock<IFeatureInfo>();
        feature.Setup(f => f.Id).Returns("OrchardCore.Admin");

        var builder = new ShapeTableBuilder(feature.Object);
        await new AdminListShapeTableProvider().DiscoverAsync(builder);

        var descriptor = new ShapeDescriptor { ShapeType = shapeType };

        foreach (var alteration in builder.BuildAlterations().Where(a => a.ShapeType == shapeType))
        {
            alteration.Alter(descriptor);
        }

        var context = new ShapeDisplayContext { Shape = shape };

        foreach (var displaying in descriptor.DisplayingAsync)
        {
            await displaying(context);
        }
    }
}
