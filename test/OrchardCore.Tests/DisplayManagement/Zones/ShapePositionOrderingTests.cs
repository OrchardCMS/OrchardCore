using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement.Zones;

public class ShapePositionOrderingTests
{
    /// <summary>
    /// Proves that shapes added to a zone are ordered by their position string,
    /// regardless of the order they were inserted (simulating different modules/projects
    /// registering shapes in arbitrary order).
    /// </summary>
    [Fact]
    public async Task Items_ShouldOrderByPosition_RegardlessOfInsertionOrder()
    {
        // Arrange - Add shapes in reverse order to prove insertion order doesn't matter.
        var zone = new Shape();
        var shape3 = new Shape { Position = "3" };
        var shape1 = new Shape { Position = "1" };
        var shape2 = new Shape { Position = "2" };

        await zone.AddAsync(shape3, "3");
        await zone.AddAsync(shape1, "1");
        await zone.AddAsync(shape2, "2");

        // Act
        var items = zone.Items;

        // Assert - Items should be in position order, not insertion order.
        Assert.Equal("1", items[0].Position);
        Assert.Equal("2", items[1].Position);
        Assert.Equal("3", items[2].Position);
    }

    /// <summary>
    /// Simulates two different projects (Project A and Project B) registering shapes.
    /// Even though Project A registers first (alphabetically earlier), the position
    /// determines the final order, not the project/registration order.
    /// </summary>
    [Fact]
    public async Task Items_ShouldOrderByPosition_NotByProjectRegistrationOrder()
    {
        // Arrange
        var zone = new Shape();

        // Simulate Project A (registered first because alphabetically shorter name)
        // placing a shape at position 5.
        var shapeFromProjectA = new Shape();
        shapeFromProjectA.Metadata.Name = "ProjectA_Shape";
        await zone.AddAsync(shapeFromProjectA, "5");

        // Simulate Project B (registered second) placing a shape at position 2.
        var shapeFromProjectB = new Shape();
        shapeFromProjectB.Metadata.Name = "ProjectB_Shape";
        await zone.AddAsync(shapeFromProjectB, "2");

        // Act
        var items = zone.Items;

        // Assert - Project B's shape should come first because position 2 < 5,
        // regardless of Project A being registered first.
        Assert.Equal(2, items.Count);
        Assert.Same(shapeFromProjectB, items[0]);
        Assert.Same(shapeFromProjectA, items[1]);
    }

    /// <summary>
    /// Proves that shapes from multiple modules with various positions
    /// are sorted purely by their position value.
    /// </summary>
    [Fact]
    public async Task Items_ShouldOrderMultipleModuleShapes_ByPositionOnly()
    {
        // Arrange - Simulate 5 different modules adding shapes in arbitrary order.
        var zone = new Shape();
        var moduleE = new Shape { Position = "1" };
        var moduleA = new Shape { Position = "5" };
        var moduleC = new Shape { Position = "3" };
        var moduleB = new Shape { Position = "2" };
        var moduleD = new Shape { Position = "4" };

        // Add in "alphabetical module name" order (A, B, C, D, E)
        // but positions are scrambled.
        await zone.AddAsync(moduleA, "5");
        await zone.AddAsync(moduleB, "2");
        await zone.AddAsync(moduleC, "3");
        await zone.AddAsync(moduleD, "4");
        await zone.AddAsync(moduleE, "1");

        // Act
        var items = zone.Items;

        // Assert - Order should be by position, not by insertion order.
        Assert.Same(moduleE, items[0]); // position 1
        Assert.Same(moduleB, items[1]); // position 2
        Assert.Same(moduleC, items[2]); // position 3
        Assert.Same(moduleD, items[3]); // position 4
        Assert.Same(moduleA, items[4]); // position 5
    }

    /// <summary>
    /// Proves that "before" position comes before all numbered positions
    /// and "after" comes after all numbered positions.
    /// </summary>
    [Fact]
    public async Task Items_BeforeShouldBeFirst_AfterShouldBeLast()
    {
        // Arrange
        var zone = new Shape();
        var afterShape = new Shape();
        var middleShape = new Shape();
        var beforeShape = new Shape();

        await zone.AddAsync(afterShape, "after");
        await zone.AddAsync(middleShape, "5");
        await zone.AddAsync(beforeShape, "before");

        // Act
        var items = zone.Items;

        // Assert
        Assert.Same(beforeShape, items[0]);
        Assert.Same(middleShape, items[1]);
        Assert.Same(afterShape, items[2]);
    }

    /// <summary>
    /// Proves the full ordering: before &lt; 0 &lt; 1 &lt; 2 &lt; ... &lt; after.
    /// </summary>
    [Fact]
    public async Task Items_ShouldOrderCorrectly_BeforeNumbersAfter()
    {
        // Arrange
        var zone = new Shape();
        var shapes = new Dictionary<string, Shape>
        {
            ["after"] = new Shape(),
            ["2"] = new Shape(),
            ["before"] = new Shape(),
            ["0"] = new Shape(),
            ["1"] = new Shape(),
            ["10"] = new Shape(),
        };

        // Add in random order.
        await zone.AddAsync(shapes["after"], "after");
        await zone.AddAsync(shapes["2"], "2");
        await zone.AddAsync(shapes["before"], "before");
        await zone.AddAsync(shapes["0"], "0");
        await zone.AddAsync(shapes["1"], "1");
        await zone.AddAsync(shapes["10"], "10");

        // Act
        var items = zone.Items;

        // Assert
        Assert.Same(shapes["before"], items[0]);
        Assert.Same(shapes["0"], items[1]);
        Assert.Same(shapes["1"], items[2]);
        Assert.Same(shapes["2"], items[3]);
        Assert.Same(shapes["10"], items[4]);
        Assert.Same(shapes["after"], items[5]);
    }

    /// <summary>
    /// Proves that a shape with no position (empty string) is treated as position "0".
    /// </summary>
    [Fact]
    public async Task Items_EmptyPosition_ShouldBeTreatedAsZero()
    {
        // Arrange
        var zone = new Shape();
        var shapeAtOne = new Shape();
        var shapeNoPosition = new Shape();
        var shapeAtTwo = new Shape();

        await zone.AddAsync(shapeAtTwo, "2");
        await zone.AddAsync(shapeNoPosition, "");
        await zone.AddAsync(shapeAtOne, "1");

        // Act
        var items = zone.Items;

        // Assert - Empty position treated as "0", so it comes first.
        Assert.Same(shapeNoPosition, items[0]);
        Assert.Same(shapeAtOne, items[1]);
        Assert.Same(shapeAtTwo, items[2]);
    }

    /// <summary>
    /// Proves that dot notation positions are supported and correctly ordered.
    /// 12.5 should come after 12.4, and both after 11.
    /// </summary>
    [Fact]
    public async Task Items_DotNotation_ShouldOrderCorrectly()
    {
        // Arrange
        var zone = new Shape();
        var shape12_5 = new Shape();
        var shape11 = new Shape();
        var shape12_4 = new Shape();
        var shape12 = new Shape();

        await zone.AddAsync(shape12_5, "12.5");
        await zone.AddAsync(shape11, "11");
        await zone.AddAsync(shape12_4, "12.4");
        await zone.AddAsync(shape12, "12");

        // Act
        var items = zone.Items;

        // Assert
        Assert.Same(shape11, items[0]);    // 11
        Assert.Same(shape12, items[1]);    // 12 (less specific, comes before 12.x)
        Assert.Same(shape12_4, items[2]);  // 12.4
        Assert.Same(shape12_5, items[3]);  // 12.5
    }

    /// <summary>
    /// Proves that sub-positions work: 1 &lt; 1.1 &lt; 1.2 &lt; 1.10 &lt; 2.
    /// </summary>
    [Fact]
    public async Task Items_SubPositions_ShouldOrderNumerically()
    {
        // Arrange
        var zone = new Shape();
        var shapes = new Dictionary<string, Shape>
        {
            ["1.10"] = new Shape(),
            ["2"] = new Shape(),
            ["1"] = new Shape(),
            ["1.2"] = new Shape(),
            ["1.1"] = new Shape(),
        };

        await zone.AddAsync(shapes["1.10"], "1.10");
        await zone.AddAsync(shapes["2"], "2");
        await zone.AddAsync(shapes["1"], "1");
        await zone.AddAsync(shapes["1.2"], "1.2");
        await zone.AddAsync(shapes["1.1"], "1.1");

        // Act
        var items = zone.Items;

        // Assert
        Assert.Same(shapes["1"], items[0]);     // 1
        Assert.Same(shapes["1.1"], items[1]);   // 1.1
        Assert.Same(shapes["1.2"], items[2]);   // 1.2
        Assert.Same(shapes["1.10"], items[3]);  // 1.10
        Assert.Same(shapes["2"], items[4]);     // 2
    }

    /// <summary>
    /// Comprehensive test combining before, after, dot notation, and numbered positions.
    /// Proves the complete ordering model works end-to-end.
    /// </summary>
    [Fact]
    public async Task Items_CompleteOrdering_ShouldWorkEndToEnd()
    {
        // Arrange
        var zone = new Shape();
        var shapes = new Dictionary<string, Shape>
        {
            ["after"] = new Shape(),
            ["3.1"] = new Shape(),
            ["before"] = new Shape(),
            ["1"] = new Shape(),
            [""] = new Shape(),      // empty = position 0
            ["2.5"] = new Shape(),
            ["2"] = new Shape(),
            ["3"] = new Shape(),
        };

        // Add in deliberately scrambled order.
        await zone.AddAsync(shapes["3.1"], "3.1");
        await zone.AddAsync(shapes["after"], "after");
        await zone.AddAsync(shapes["1"], "1");
        await zone.AddAsync(shapes[""], "");
        await zone.AddAsync(shapes["before"], "before");
        await zone.AddAsync(shapes["2.5"], "2.5");
        await zone.AddAsync(shapes["3"], "3");
        await zone.AddAsync(shapes["2"], "2");

        // Act
        var items = zone.Items;

        // Assert - Expected order: before, 0(empty), 1, 2, 2.5, 3, 3.1, after
        Assert.Equal(8, items.Count);
        Assert.Same(shapes["before"], items[0]);
        Assert.Same(shapes[""], items[1]);       // empty = 0
        Assert.Same(shapes["1"], items[2]);
        Assert.Same(shapes["2"], items[3]);
        Assert.Same(shapes["2.5"], items[4]);
        Assert.Same(shapes["3"], items[5]);
        Assert.Same(shapes["3.1"], items[6]);
        Assert.Same(shapes["after"], items[7]);
    }

    /// <summary>
    /// Proves that shapes with identical positions maintain stable order,
    /// but the position itself is the primary sort key.
    /// </summary>
    [Fact]
    public async Task Items_SamePosition_ShouldMaintainStableOrder()
    {
        // Arrange
        var zone = new Shape();
        var first = new Shape();
        first.Metadata.Name = "First";
        var second = new Shape();
        second.Metadata.Name = "Second";
        var third = new Shape();
        third.Metadata.Name = "Third";

        // All at the same position.
        await zone.AddAsync(first, "1");
        await zone.AddAsync(second, "1");
        await zone.AddAsync(third, "1");

        // Act
        var items = zone.Items;

        // Assert - All at same position, insertion order is preserved (stable sort).
        Assert.Equal(3, items.Count);
        Assert.Same(first, items[0]);
        Assert.Same(second, items[1]);
        Assert.Same(third, items[2]);
    }

    /// <summary>
    /// Proves that deeply nested dot notation (e.g., 1.2.3) is supported.
    /// </summary>
    [Fact]
    public async Task Items_DeepDotNotation_ShouldOrderCorrectly()
    {
        // Arrange
        var zone = new Shape();
        var shape1_2_3 = new Shape();
        var shape1_2_1 = new Shape();
        var shape1_1 = new Shape();
        var shape1_2 = new Shape();

        await zone.AddAsync(shape1_2_3, "1.2.3");
        await zone.AddAsync(shape1_2_1, "1.2.1");
        await zone.AddAsync(shape1_1, "1.1");
        await zone.AddAsync(shape1_2, "1.2");

        // Act
        var items = zone.Items;

        // Assert
        Assert.Same(shape1_1, items[0]);    // 1.1
        Assert.Same(shape1_2, items[1]);    // 1.2
        Assert.Same(shape1_2_1, items[2]);  // 1.2.1
        Assert.Same(shape1_2_3, items[3]);  // 1.2.3
    }
}
