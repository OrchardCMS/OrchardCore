using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Tests.DisplayManagement.Decriptors;

public class LocationParserTests
{
    [Theory]
    [InlineData("Content")]
    [InlineData("Content:5")]
    [InlineData("Content:5#Tab1")]
    [InlineData("Content:5@Group1")]
    [InlineData("Content:5@Group1#Tab1")]
    [InlineData("/Content")]
    [InlineData("/Content:5")]
    [InlineData("/Content:5#Tab1")]
    [InlineData("/Content:5#Card1")]
    [InlineData("/Content:5#Col1")]
    [InlineData("/Content:5@Group1")]
    [InlineData("/Content:5@Group1%Card1")]
    [InlineData("/Content:5@Group1#Tab1")]
    [InlineData("/Content:5@Group1#Tab1%Card1")]
    [InlineData("/Content:5@Group1#Tab1%Card1|Col1")]
    public void ZoneShouldBeParsed(string location)
    {
        Assert.Equal("Content", new PlacementInfo(location).Zones.FirstOrDefault());
    }

    [Theory]
    [InlineData("Content", "")]
    [InlineData("Content:5", "5")]
    [InlineData("Content:5%Card1", "5")]
    [InlineData("Content:5%Col1", "5")]
    [InlineData("Content:5#Tab1", "5")]
    [InlineData("Content:5#Tab1%Card1", "5")]
    [InlineData("Content:5#Tab1|Col1", "5")]
    [InlineData("Content:5.1#Tab1", "5.1")]
    [InlineData("Content:5@Group1", "5")]
    [InlineData("Content:5@Group1%Card1", "5")]
    [InlineData("Content:5@Group1|Col1", "5")]
    [InlineData("Content:5@Group1#Tab1", "5")]
    [InlineData("Content:5@Group1#Tab1%Card1", "5")]
    [InlineData("Content:5@Group1#Tab1|Col1", "5")]
    [InlineData("/Content", "")]
    [InlineData("/Content:5", "5")]
    [InlineData("/Content:5#Tab1", "5")]
    [InlineData("/Content:5%Card1", "5")]
    [InlineData("/Content:5|Col1", "5")]
    [InlineData("/Content:5.1#Tab1", "5.1")]
    [InlineData("/Content:5.1%Card1", "5.1")]
    [InlineData("/Content:5.1|Col1", "5.1")]
    [InlineData("/Content:5@Group1", "5")]
    [InlineData("/Content:5@Group1%Card1", "5")]
    [InlineData("/Content:5@Group1|Col1", "5")]
    [InlineData("/Content:5@Group1#Tab1", "5")]
    [InlineData("/Content:5@Group1#Tab1%Card1", "5")]
    [InlineData("/Content:5@Group1#Tab1|Col1", "5")]
    [InlineData("/Content:5@Group1#Tab1%Card1|Col1", "5")]
    public void PositionShouldBeParsed(string location, string expectedPosition)
    {
        Assert.Equal(expectedPosition, new PlacementInfo(location).Position);
    }

    [Theory]
    [InlineData("Content", false)]
    [InlineData("Content:5", false)]
    [InlineData("Content:5#Tab1", false)]
    [InlineData("Content:5%Card1", false)]
    [InlineData("Content:5|Col1", false)]
    [InlineData("Content:5.1#Tab1", false)]
    [InlineData("Content:5.1%Card1", false)]
    [InlineData("Content:5.1#Col1", false)]
    [InlineData("Content:5@Group1", false)]
    [InlineData("Content:5@Group1%Card1", false)]
    [InlineData("Content:5@Group1|Col1", false)]
    [InlineData("Content:5@Group1#Tab1", false)]
    [InlineData("Content:5@Group1#Tab1|Card1", false)]
    [InlineData("Content:5@Group1#Tab1%Col1", false)]
    [InlineData("/Content", true)]
    [InlineData("/Content:5", true)]
    [InlineData("/Content:5#Tab1", true)]
    [InlineData("/Content:5%Card1", true)]
    [InlineData("/Content:5|Col1", true)]
    [InlineData("/Content:5.1#Tab1", true)]
    [InlineData("/Content:5.1%Card1", true)]
    [InlineData("/Content:5.1|Col1", true)]
    [InlineData("/Content:5@Group1", true)]
    [InlineData("/Content:5@Group1%Card1", true)]
    [InlineData("/Content:5@Group1|Col1", true)]
    [InlineData("/Content:5@Group1%Card1|Col1", true)]
    [InlineData("/Content:5@Group1#Tab1", true)]
    [InlineData("/Content:5@Group1#Tab1%Card1", true)]
    [InlineData("/Content:5@Group1#Tab1|Col1", true)]
    [InlineData("/Content:5@Group1#Tab1|Card1%Col1", true)]
    public void LayoutZoneShouldBeParsed(string location, bool expectedIsLayoutZone)
    {
        Assert.Equal(expectedIsLayoutZone, new PlacementInfo(location).IsLayoutZone());
    }

    [Theory]
    [InlineData("Content", "")]
    [InlineData("Content:5", "")]
    [InlineData("Content:5#Tab1", "Tab1")]
    [InlineData("Content:5.1#Tab1", "Tab1")]
    [InlineData("Content:5#Tab1%Card1", "Tab1")]
    [InlineData("Content:5.1#Tab1|Card1", "Tab1")]
    [InlineData("Content:5#Tab1|Col1", "Tab1")]
    [InlineData("Content:5.1#Tab1|Col1", "Tab1")]
    [InlineData("Content:5#Tab1%Card1|Col1", "Tab1")]
    [InlineData("Content:5.1#Tab1%Card1|Col1", "Tab1")]
    [InlineData("Content:5@Group1", "")]
    [InlineData("Content:5@Group1%Card1", "")]
    [InlineData("Content:5@Group1|Col1", "")]
    [InlineData("Content:5@Group1%Card1|Col1", "")]
    [InlineData("Content:5@Group1#Tab1", "Tab1")]
    [InlineData("Content:5@Group1#Tab1%Card1", "Tab1")]
    [InlineData("Content:5@Group1#Tab1|Col1", "Tab1")]
    [InlineData("Content:5@Group1#Tab1%Card1|Col1", "Tab1")]
    [InlineData("/Content", "")]
    [InlineData("/Content:5", "")]
    [InlineData("/Content:5#Tab1", "Tab1")]
    [InlineData("/Content:5.1#Tab1", "Tab1")]
    [InlineData("/Content:5#Tab1%Card1", "Tab1")]
    [InlineData("/Content:5.1#Tab1|Card1", "Tab1")]
    [InlineData("/Content:5#Tab1%Col1", "Tab1")]
    [InlineData("/Content:5.1#Tab1%Col1", "Tab1")]
    [InlineData("/Content:5#Tab1%Card1|Col1", "Tab1")]
    [InlineData("/Content:5.1#Tab1%Card1|Col1", "Tab1")]
    [InlineData("/Content:5@Group1", "")]
    [InlineData("/Content:5@Group1%Card1", "")]
    [InlineData("/Content:5@Group1|Col1", "")]
    [InlineData("/Content:5@Group1#Tab1", "Tab1")]
    [InlineData("/Content:5@Group1#Tab1%Card1", "Tab1")]
    [InlineData("/Content:5@Group1#Tab1|Col1", "Tab1")]
    [InlineData("/Content:5@Group1#Tab1%Card1|Col1", "Tab1")]
    [InlineData("/Content:5#Tab1@Group1", "Tab1")]
    [InlineData("/Content:5#Tab1@Group1%Card1", "Tab1")]
    [InlineData("/Content:5#Tab1@Group1|Col1", "Tab1")]
    [InlineData("/Content:5#Tab1@Group1%Card1|Col1", "Tab1")]
    public void TabShouldBeParsed(string location, string expectedTab)
    {
        Assert.Equal(expectedTab, new PlacementInfo(location).Tab);
    }

    [Theory]
    [InlineData("Content", "")]
    [InlineData("Content:5", "5")]
    [InlineData("Content:5.1", "5.1")]
    [InlineData("Content:5.1.2", "5.1.2")]
    [InlineData("Content:before", "before")]
    [InlineData("Content:after", "after")]
    [InlineData("Content:before#Tab1", "before")]
    [InlineData("Content:after#Tab1", "after")]
    public void PositionShouldSupportSpecialAndDotFormats(string location, string expectedPosition)
    {
        Assert.Equal(expectedPosition, new PlacementInfo(location).Position);
    }

    [Theory]
    [InlineData("Content", null, "")]
    [InlineData("Content", "", "")]
    [InlineData("Content", "5", "5")]
    [InlineData("Content", "0", "0")]
    [InlineData("Content:3", "5", "3")]
    public void Position_ShouldUseDefaultPosition_WhenNoExplicitPosition(string location, string defaultPosition, string expectedPosition)
    {
        // When Location has no ':' delimiter, Position returns DefaultPosition ?? "".
        // When Location has a ':' delimiter, the explicit position takes precedence.
        var placement = new PlacementInfo(location, defaultPosition: defaultPosition);
        Assert.Equal(expectedPosition, placement.Position);
    }

    [Theory]
    [InlineData("Content", null)]
    [InlineData("Content:5", null)]
    [InlineData("Content:5#Tab1", null)]
    [InlineData("Content:5.1#Tab1", null)]
    [InlineData("Content:5%Card1", null)]
    [InlineData("Content:5.1|Col1", null)]
    [InlineData("Content:5#Tab1%Card1", null)]
    [InlineData("Content:5.1#Tab1|Col1", null)]
    [InlineData("Content:5#Tab1%Card1|Col1", null)]
    [InlineData("Content:5.1#Tab1%Card1|Col1", null)]
    [InlineData("Content:5@Group1", "Group1")]
    [InlineData("Content:5@Group1%Card1", "Group1")]
    [InlineData("Content:5@Group1%Col1", "Group1")]
    [InlineData("Content:5@Group1#Tab1", "Group1")]
    [InlineData("Content:5@Group1#Tab1%Card1", "Group1")]
    [InlineData("Content:5@Group1#Tab1|Col1", "Group1")]
    [InlineData("Content:5@Group1#Tab1%Card1|Col1", "Group1")]
    [InlineData("Content:5#Tab1@Group1", "Group1")]
    [InlineData("Content:5#Tab1@Group1%Card1", "Group1")]
    [InlineData("Content:5#Tab1@Group1|Col1", "Group1")]
    [InlineData("Content:5#Tab1@Group1%Card1|Col1", "Group1")]
    [InlineData("/Content", null)]
    [InlineData("/Content:5", null)]
    [InlineData("/Content:5#Tab1", null)]
    [InlineData("/Content:5.1#Tab1", null)]
    [InlineData("/Content:5%Card1", null)]
    [InlineData("/Content:5.1%Card1", null)]
    [InlineData("/Content:5|Col1", null)]
    [InlineData("/Content:5.1|Col1", null)]
    [InlineData("/Content:5%Card1|Col1", null)]
    [InlineData("/Content:5.1%Card1|Col1", null)]
    [InlineData("/Content:5@Group1", "Group1")]
    [InlineData("/Content:5@Group1%Card1", "Group1")]
    [InlineData("/Content:5@Group1|Col1", "Group1")]
    [InlineData("/Content:5@Group1%Card1|Col1", "Group1")]
    [InlineData("/Content:5@Group1#Tab1", "Group1")]
    [InlineData("/Content:5@Group1#Tab1%Card1", "Group1")]
    [InlineData("/Content:5@Group1#Tab1|Col1", "Group1")]
    [InlineData("/Content:5@Group1#Tab1%Card1|Col1", "Group1")]
    [InlineData("/Content:5#Tab1@Group1", "Group1")]
    [InlineData("/Content:5#Tab1@Group1%Card1", "Group1")]
    [InlineData("/Content:5#Tab1@Group1|Col1", "Group1")]
    [InlineData("/Content:5#Tab1@Group1%Card1|Col1", "Group1")]
    public void GroupShouldBeParsed(string location, string expectedGroup)
    {
        Assert.Equal(expectedGroup, new PlacementInfo(location).Group);
    }

    [Theory]
    [InlineData("Content", null)]
    [InlineData("Content:5", null)]
    [InlineData("Content:5#Tab1", null)]
    [InlineData("Content:5.1#Tab1", null)]
    [InlineData("Content:5@Group1", null)]
    [InlineData("Content:5#Tab1@Group1", null)]
    [InlineData("Content:5.1#Tab1@Group1", null)]
    [InlineData("Content:5#Tab1@Group1|Col1", null)]
    [InlineData("Content:5.1#Tab1@Group1|Col1", null)]
    [InlineData("Content:5%Card1", "Card1")]
    [InlineData("Content:5%Card1@Group1", "Card1")]
    [InlineData("Content:5%Card1%Col1", "Card1")]
    [InlineData("Content:5%Card1#Tab1", "Card1")]
    [InlineData("Content:5%Card1#Tab1@Group1", "Card1")]
    [InlineData("Content:5%Card1#Tab1|Col1", "Card1")]
    [InlineData("Content:5%Card1#Tab1@Group1|Col1", "Card1")]
    [InlineData("Content:5#Tab1%Card1", "Card1")]
    [InlineData("Content:5#Tab1%Card1@Group1", "Card1")]
    [InlineData("Content:5#Tab1%Card1|Col1", "Card1")]
    [InlineData("Content:5#Tab1%Card1@Group1|Col1", "Card1")]
    [InlineData("/Content", null)]
    [InlineData("/Content:5", null)]
    [InlineData("/Content:5#Tab1", null)]
    [InlineData("/Content:5.1#Tab1", null)]
    [InlineData("/Content:5@Group1", null)]
    [InlineData("/Content:5.1@Group1", null)]
    [InlineData("/Content:5|Col1", null)]
    [InlineData("/Content:5.1|Col1", null)]
    [InlineData("/Content:5%Card1|Col1", "Card1")]
    [InlineData("/Content:5.1%Card1|Col1", "Card1")]
    [InlineData("/Content:5%Card1", "Card1")]
    [InlineData("/Content:5%Card1@Group1", "Card1")]
    [InlineData("/Content:5%Card1#Tab1", "Card1")]
    [InlineData("/Content:5%Card1#Tab1@Group1", "Card1")]
    [InlineData("/Content:5%Card1#Tab1|Col1", "Card1")]
    [InlineData("/Content:5%Card1#Tab1@Group1|Col1", "Card1")]
    [InlineData("/Content:5#Tab1%Card1", "Card1")]
    [InlineData("/Content:5#Tab1%Card1@Group1", "Card1")]
    [InlineData("/Content:5#Tab1%Card1|Col1", "Card1")]
    [InlineData("/Content:5#Tab1%Card1@Group1|Col1", "Card1")]
    public void CardShouldBeParsed(string location, string expectedCard)
    {
        Assert.Equal(expectedCard, new PlacementInfo(location).Card);
    }

    [Theory]
    [InlineData("Content", null)]
    [InlineData("Content:5", null)]
    [InlineData("Content:5#Tab1", null)]
    [InlineData("Content:5.1#Tab1", null)]
    [InlineData("Content:5@Group1", null)]
    [InlineData("Content:5#Tab1@Group1", null)]
    [InlineData("Content:5.1#Tab1@Group1", null)]
    [InlineData("Content:5#Tab1@Group1%Card1", null)]
    [InlineData("Content:5.1#Tab1@Group1%Card", null)]
    [InlineData("Content:5|Col1", "Col1")]
    [InlineData("Content:5@Group1|Col1", "Col1")]
    [InlineData("Content:5@Group1%Card1|Col1", "Col1")]
    [InlineData("Content:5|Col1#Tab1", "Col1")]
    [InlineData("Content:5|Col1#Tab1%Card1", "Col1")]
    [InlineData("Content:5@Group1#Tab1|Col1", "Col1")]
    [InlineData("Content:5@Group1#Tab1%Card1|Col1", "Col1")]
    [InlineData("Content:5#Tab1|Col1", "Col1")]
    [InlineData("Content:5#Tab1%Card1|Col1", "Col1")]
    [InlineData("Content:5#Tab1@Group1|Col1", "Col1")]
    [InlineData("Content:5#Tab1@Group1%Card1|Col1", "Col1")]
    [InlineData("/Content", null)]
    [InlineData("/Content:5", null)]
    [InlineData("/Content:5#Tab1", null)]
    [InlineData("/Content:5.1#Tab1", null)]
    [InlineData("/Content:5%Card1", null)]
    [InlineData("/Content:5.1%Card1", null)]
    [InlineData("/Content:5@Group1", null)]
    [InlineData("/Content:5.1@Group1", null)]
    [InlineData("/Content:5%Card1@Group1", null)]
    [InlineData("/Content:5.1%Card1@Group1", null)]
    [InlineData("/Content:5|Col1", "Col1")]
    [InlineData("/Content:5%Card1|Col1", "Col1")]
    [InlineData("/Content:5@Group1|Col1", "Col1")]
    [InlineData("/Content:5@Group1%Card1|Col1", "Col1")]
    [InlineData("/Content:5|Col1#Tab1", "Col1")]
    [InlineData("/Content:5|Col1#Tab1%Card1", "Col1")]
    [InlineData("/Content:5@Group1#Tab1|Col1", "Col1")]
    [InlineData("/Content:5@Group1#Tab1%Card1|Col1", "Col1")]
    [InlineData("/Content:5#Tab1|Col1", "Col1")]
    [InlineData("/Content:5#Tab1%Card1|Col1", "Col1")]
    [InlineData("/Content:5#Tab1@Group1|Col1", "Col1")]
    [InlineData("/Content:5#Tab1%Card1@Group1|Col1", "Col1")]
    public void ColumnShouldBeParsed(string location, string expectedColumn)
    {
        Assert.Equal(expectedColumn, new PlacementInfo(location).Column);
    }

    [Fact]
    public void WithDefaults_ShouldReturnSameInstance_WhenNoChangesNeeded()
    {
        var placement = new PlacementInfo("Content:5#Tab1@Group1%Card1|Col1");

        var result = placement.WithDefaults("Content:5#Tab1@Group1%Card1|Col1", null);

        Assert.Same(placement, result);
    }

    [Fact]
    public void WithDefaults_ShouldReturnSameInstance_WhenLocationAndPositionAlreadySet()
    {
        var placement = new PlacementInfo("Content:5#Tab1", defaultPosition: "10");

        var result = placement.WithDefaults("OtherZone", "20");

        Assert.Same(placement, result);
    }

    [Fact]
    public void WithDefaults_ShouldUpdateDefaultPosition_WhenNotSet()
    {
        var placement = new PlacementInfo("Content:5#Tab1@Group1%Card1|Col1");

        var result = placement.WithDefaults(null, "10");

        Assert.NotSame(placement, result);
        Assert.Equal("Content:5#Tab1@Group1%Card1|Col1", result.Location);
        Assert.Equal("10", result.DefaultPosition);
        // Position property should still return "5" because explicit position takes precedence.
        Assert.Equal("5", result.Position);
        // Parsed values should be preserved.
        Assert.Equal("Tab1", result.Tab);
        Assert.Equal("Group1", result.Group);
        Assert.Equal("Card1", result.Card);
        Assert.Equal("Col1", result.Column);
        Assert.Equal(["Content"], result.Zones);
    }

    [Fact]
    public void WithDefaults_ShouldUpdateLocation_WhenNotSet()
    {
        var placement = new PlacementInfo(location: null);

        var result = placement.WithDefaults("Content:5#Tab1@Group1%Card1|Col1", null);

        Assert.NotSame(placement, result);
        Assert.Equal("Content:5#Tab1@Group1%Card1|Col1", result.Location);
        // Parsed values should be populated from the new location.
        Assert.Equal("5", result.Position);
        Assert.Equal("Tab1", result.Tab);
        Assert.Equal("Group1", result.Group);
        Assert.Equal("Card1", result.Card);
        Assert.Equal("Col1", result.Column);
        Assert.Equal(["Content"], result.Zones);
    }

    [Fact]
    public void WithDefaults_ShouldPreserveDefaultPosition_WhenOnlyLocationChanges()
    {
        var placement = new PlacementInfo(location: null, defaultPosition: "existing");

        var result = placement.WithDefaults("Content:5", null);

        Assert.NotSame(placement, result);
        Assert.Equal("Content:5", result.Location);
        Assert.Equal("existing", result.DefaultPosition);
    }

    [Fact]
    public void WithDefaults_ShouldUpdateBothLocationAndDefaultPosition()
    {
        var placement = new PlacementInfo(location: null, defaultPosition: null);

        var result = placement.WithDefaults("Content#Tab1", "10");

        Assert.NotSame(placement, result);
        Assert.Equal("Content#Tab1", result.Location);
        Assert.Equal("10", result.DefaultPosition);
        // Position should use DefaultPosition since no explicit position in location.
        Assert.Equal("10", result.Position);
        Assert.Equal("Tab1", result.Tab);
        Assert.Equal(["Content"], result.Zones);
    }
}
