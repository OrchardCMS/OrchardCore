using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Templates.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Templates;

public class ShapeInspectorTests
{
    [Fact]
    public async Task Stop_RenderedShapes_ListsEachShapeBeforeItsChildrenWithAlternates()
    {
        var inspector = new ShapeInspector();
        var content = CreateContext("Content", "Detail", "Content__BlogPost", "Content__Alias__about");
        var field = CreateContext("TextField", "Detail", "TextField_Detail", "BlogPost__TextField");

        inspector.Start();
        await inspector.DisplayingAsync(content);
        await inspector.DisplayingAsync(field);
        await inspector.DisplayingFinalizedAsync(field);
        await inspector.DisplayingFinalizedAsync(content);
        var shapes = inspector.Stop();

        Assert.Collection(
            shapes,
            shape =>
            {
                Assert.Equal("Content", shape.Type);
                Assert.Equal(0, shape.Depth);
                Assert.Equal(["Content__Alias__about", "Content__BlogPost"], shape.Alternates);
            },
            shape =>
            {
                Assert.Equal("TextField", shape.Type);
                Assert.Equal(1, shape.Depth);
                Assert.Equal(["BlogPost__TextField", "TextField_Detail"], shape.Alternates);
            });
    }

    [Fact]
    public async Task DisplayingAsync_NotStarted_RecordsNothing()
    {
        var inspector = new ShapeInspector();
        var context = CreateContext("Content", "Detail");

        await inspector.DisplayingAsync(context);
        await inspector.DisplayingFinalizedAsync(context);

        Assert.Empty(inspector.Stop());
    }

    private static ShapeDisplayContext CreateContext(string type, string displayType, params string[] alternates)
    {
        var shape = new Shape();
        shape.Metadata.Type = type;
        shape.Metadata.DisplayType = displayType;
        shape.Metadata.Alternates.AddRange(alternates);

        return new ShapeDisplayContext { Shape = shape };
    }
}
