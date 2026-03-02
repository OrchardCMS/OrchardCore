using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement.Decriptors;

public class PlacementInfoExtensionsCombineTests
{
    #region Combine - Null Handling

    [Fact]
    public void Combine_BothNull_ShouldReturnNull()
    {
        // Act
        PlacementInfo first = null;
        var result = PlacementInfoExtensions.Combine(first, null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Combine_FirstNull_ShouldReturnSecond()
    {
        // Arrange
        var second = new PlacementInfo("Content:1");

        // Act
        var result = PlacementInfoExtensions.Combine(null, second);

        // Assert
        Assert.Same(second, result);
    }

    [Fact]
    public void Combine_SecondNull_ShouldReturnFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content:1");

        // Act
        var result = PlacementInfoExtensions.Combine(first, null);

        // Assert
        Assert.Same(first, result);
    }

    #endregion

    #region Combine - Location Overriding (Last Non-Empty Wins)

    [Fact]
    public void Combine_SecondHasLocation_ShouldOverrideFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content:1");
        var second = new PlacementInfo("Content:5");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert - Second (last) wins.
        Assert.Equal("Content:5", result.Location);
    }

    [Fact]
    public void Combine_SecondHasEmptyLocation_ShouldKeepFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content:1");
        var second = new PlacementInfo("");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert - First is preserved when second is empty.
        Assert.Equal("Content:1", result.Location);
    }

    [Fact]
    public void Combine_SecondHasNullLocation_ShouldKeepFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content:1");
        var second = new PlacementInfo(null);

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Equal("Content:1", result.Location);
    }

    /// <summary>
    /// Simulates multiple providers being aggregated. The last provider
    /// with a non-empty Location wins, matching BaseDisplayManager.FindPlacementImpl behavior.
    /// </summary>
    [Fact]
    public void Combine_MultipleProviders_LastNonEmptyLocationWins()
    {
        // Arrange - Simulating 3 providers.
        var providerA = new PlacementInfo("Content:1");
        var providerB = new PlacementInfo("Content:5");
        var providerC = new PlacementInfo("Content:3");

        // Act - Aggregate left-to-right as BaseDisplayManager does.
        var result = new[] { providerA, providerB, providerC }
            .Aggregate<PlacementInfo, PlacementInfo>(null, (prev, current) =>
                PlacementInfoExtensions.Combine(prev, current));

        // Assert - Last provider (C) wins since it has a non-empty Location.
        Assert.Equal("Content:3", result.Location);
    }

    /// <summary>
    /// When a later provider returns empty Location, the previous non-empty Location is preserved.
    /// This proves that only providers with explicit locations can override.
    /// </summary>
    [Fact]
    public void Combine_LaterProviderEmptyLocation_ShouldNotOverride()
    {
        // Arrange
        var providerA = new PlacementInfo("Content:1");
        var providerB = new PlacementInfo("");
        var providerC = new PlacementInfo(null);

        // Act
        var result = new[] { providerA, providerB, providerC }
            .Aggregate<PlacementInfo, PlacementInfo>(null, (prev, current) =>
                PlacementInfoExtensions.Combine(prev, current));

        // Assert - Provider A's location is preserved.
        Assert.Equal("Content:1", result.Location);
    }

    #endregion

    #region Combine - ShapeType Overriding

    [Fact]
    public void Combine_SecondHasShapeType_ShouldOverrideFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content", shapeType: "TypeA");
        var second = new PlacementInfo("Content", shapeType: "TypeB");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Equal("TypeB", result.ShapeType);
    }

    [Fact]
    public void Combine_SecondHasEmptyShapeType_ShouldKeepFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content", shapeType: "TypeA");
        var second = new PlacementInfo("Content", shapeType: "");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Equal("TypeA", result.ShapeType);
    }

    #endregion

    #region Combine - DefaultPosition Overriding

    [Fact]
    public void Combine_SecondHasDefaultPosition_ShouldOverrideFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content", defaultPosition: "1");
        var second = new PlacementInfo("Content", defaultPosition: "5");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Equal("5", result.DefaultPosition);
    }

    [Fact]
    public void Combine_SecondHasEmptyDefaultPosition_ShouldKeepFirst()
    {
        // Arrange
        var first = new PlacementInfo("Content", defaultPosition: "1");
        var second = new PlacementInfo("Content", defaultPosition: "");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Equal("1", result.DefaultPosition);
    }

    #endregion

    #region Combine - Source Tracking

    [Fact]
    public void Combine_ShouldConcatenateSources()
    {
        // Arrange
        var first = new PlacementInfo("Content", source: "ModuleA");
        var second = new PlacementInfo("Content", source: "ModuleB");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Equal("ModuleA,ModuleB", result.Source);
    }

    [Fact]
    public void Combine_MultipleProviders_ShouldTrackAllSources()
    {
        // Arrange
        var providerA = new PlacementInfo("Content:1", source: "ProjectA");
        var providerB = new PlacementInfo("Content:2", source: "ThemeB");
        var providerC = new PlacementInfo("Content:3", source: "ModuleC");

        // Act
        var result = new[] { providerA, providerB, providerC }
            .Aggregate<PlacementInfo, PlacementInfo>(null, (prev, current) =>
                PlacementInfoExtensions.Combine(prev, current));

        // Assert
        Assert.Equal("ProjectA,ThemeB,ModuleC", result.Source);
    }

    #endregion

    #region Combine - Alternates and Wrappers Merging

    [Fact]
    public void Combine_ShouldMergeAlternates()
    {
        // Arrange
        var first = new PlacementInfo("Content", alternates: ["Alt1"]);
        var second = new PlacementInfo("Content", alternates: ["Alt2"]);

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert - Both alternates should be present.
        Assert.Contains("Alt1", result.Alternates);
        Assert.Contains("Alt2", result.Alternates);
    }

    [Fact]
    public void Combine_ShouldMergeWrappers()
    {
        // Arrange
        var first = new PlacementInfo("Content", wrappers: ["Wrapper1"]);
        var second = new PlacementInfo("Content", wrappers: ["Wrapper2"]);

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert - Both wrappers should be present.
        Assert.Contains("Wrapper1", result.Wrappers);
        Assert.Contains("Wrapper2", result.Wrappers);
    }

    [Fact]
    public void Combine_NullAlternates_ShouldHandleGracefully()
    {
        // Arrange
        var first = new PlacementInfo("Content", alternates: null);
        var second = new PlacementInfo("Content", alternates: ["Alt1"]);

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert
        Assert.Contains("Alt1", result.Alternates);
    }

    #endregion

    #region Combine - Creates New Instance

    [Fact]
    public void Combine_ShouldReturnNewInstance_WhenBothNonNull()
    {
        // Arrange
        var first = new PlacementInfo("Content:1");
        var second = new PlacementInfo("Content:2");

        // Act
        var result = PlacementInfoExtensions.Combine(first, second);

        // Assert - Result should be a new instance.
        Assert.NotSame(first, result);
        Assert.NotSame(second, result);
    }

    #endregion
}
