using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Tests.DisplayManagement.Decriptors;

public class PlacementLocationBuilderTests
{
    #region Zone Tests

    [Fact]
    public void Build_ZoneOnly_ShouldReturnZoneName()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .ToString();

        Assert.Equal("Content", result);
    }

    [Fact]
    public void Build_ZoneWithDotNotation_ShouldReturnDottedZone()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content.Metadata")
            .ToString();

        Assert.Equal("Content.Metadata", result);
    }

    [Fact]
    public void Build_NoZone_ShouldThrowInvalidOperationException()
    {
        var builder = new PlacementLocationBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.ToString());
    }

    [Fact]
    public void Build_NullZone_ShouldThrowArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => new PlacementLocationBuilder().Zone(null));
    }

    [Fact]
    public void Build_EmptyZone_ShouldThrowArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => new PlacementLocationBuilder().Zone(""));
    }

    #endregion

    #region Zone Position Tests

    [Theory]
    [InlineData("1", "Content:1")]
    [InlineData("5", "Content:5")]
    [InlineData("5.1", "Content:5.1")]
    [InlineData("before", "Content:before")]
    [InlineData("after", "Content:after")]
    public void Build_ZoneWithPosition_ShouldReturnCorrectString(string position, string expected)
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", position)
            .ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Build_ZoneWithNullPosition_ShouldReturnZoneOnly()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", null)
            .ToString();

        Assert.Equal("Content", result);
    }

    #endregion

    #region Tab Tests

    [Fact]
    public void Build_ZoneWithTab_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Tab("Settings")
            .ToString();

        Assert.Equal("Content#Settings", result);
    }

    [Fact]
    public void Build_ZoneWithTabAndTabPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Tab("Settings", "1")
            .ToString();

        Assert.Equal("Content#Settings;1", result);
    }

    [Fact]
    public void Build_ZonePositionAndTab_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parameters", "1")
            .Tab("Settings", "1")
            .ToString();

        Assert.Equal("Parameters:1#Settings;1", result);
    }

    [Fact]
    public void Build_NullTab_ShouldThrowArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new PlacementLocationBuilder().Zone("Content").Tab(null));
    }

    #endregion

    #region Group Tests

    [Fact]
    public void Build_ZoneWithGroup_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Group("search")
            .ToString();

        Assert.Equal("Content:5@search", result);
    }

    [Fact]
    public void Build_NullGroup_ShouldThrowArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new PlacementLocationBuilder().Zone("Content").Group(null));
    }

    #endregion

    #region Card Tests

    [Fact]
    public void Build_ZoneWithCard_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Card("Details")
            .ToString();

        Assert.Equal("Content%Details", result);
    }

    [Fact]
    public void Build_ZoneWithCardAndCardPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Card("Details", "2")
            .ToString();

        Assert.Equal("Content%Details;2", result);
    }

    [Fact]
    public void Build_NullCard_ShouldThrowArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new PlacementLocationBuilder().Zone("Content").Card(null));
    }

    #endregion

    #region Column Tests

    [Fact]
    public void Build_ZoneWithColumn_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left")
            .ToString();

        Assert.Equal("Content|Left", result);
    }

    [Fact]
    public void Build_ZoneWithColumnAndWidth_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", width: "9")
            .ToString();

        Assert.Equal("Content|Left_9", result);
    }

    [Fact]
    public void Build_ZoneWithColumnPositionAndWidth_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", position: "1", width: "9")
            .ToString();

        Assert.Equal("Content|Left_9;1", result);
    }

    [Fact]
    public void Build_ZoneWithColumnAndPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", position: "1")
            .ToString();

        Assert.Equal("Content|Left;1", result);
    }

    [Fact]
    public void Build_ColumnWithBreakpointWidth_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", width: "lg-9")
            .ToString();

        Assert.Equal("Content|Left_lg-9", result);
    }

    [Fact]
    public void Build_NullColumn_ShouldThrowArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            new PlacementLocationBuilder().Zone("Content").Column(null));
    }

    #endregion

    #region Layout Zone Tests

    [Fact]
    public void Build_AsLayoutZone_ShouldPrefixWithSlash()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .AsLayoutZone()
            .ToString();

        Assert.Equal("/Content", result);
    }

    [Fact]
    public void Build_LayoutZoneWithPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .AsLayoutZone()
            .ToString();

        Assert.Equal("/Content:5", result);
    }

    #endregion

    #region Nesting Hierarchy Tests (Tab → Card → Column)

    [Fact]
    public void Build_TabThenCard_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1", "1")
            .Card("Card1", "2")
            .ToString();

        Assert.Equal("Content:5#Tab1;1%Card1;2", result);
    }

    [Fact]
    public void Build_TabThenColumn_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1", "1")
            .Column("Col1", "2")
            .ToString();

        Assert.Equal("Content:5#Tab1;1|Col1;2", result);
    }

    [Fact]
    public void Build_CardThenColumn_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Card("Card1", "1")
            .Column("Col1", "2", "9")
            .ToString();

        Assert.Equal("Content:5%Card1;1|Col1_9;2", result);
    }

    [Fact]
    public void Build_TabThenCardThenColumn_FullNesting()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1", "1")
            .Card("Card1", "2")
            .Column("Col1", "3", "9")
            .ToString();

        Assert.Equal("Content:5#Tab1;1%Card1;2|Col1_9;3", result);
    }

    [Fact]
    public void Build_CardWithoutTab_ShouldWork()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Card("Details")
            .ToString();

        Assert.Equal("Content:5%Details", result);
    }

    [Fact]
    public void Build_ColumnWithoutCardOrTab_ShouldWork()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Column("Left", "1", "6")
            .ToString();

        Assert.Equal("Content:5|Left_6;1", result);
    }

    #endregion

    #region Group at Different Nesting Levels

    [Fact]
    public void Build_GroupOnZone_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Group("search")
            .ToString();

        Assert.Equal("Content:5@search", result);
    }

    [Fact]
    public void Build_GroupOnTab_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings")
            .Group("search")
            .ToString();

        Assert.Equal("Content:5#Settings@search", result);
    }

    [Fact]
    public void Build_GroupOnCard_ShouldReturnCorrectString()
    {
        // Group always appears between tab and card in the serialized string.
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings")
            .Card("Details")
            .Group("search")
            .ToString();

        Assert.Equal("Content:5#Settings@search%Details", result);
    }

    [Fact]
    public void Build_GroupOnColumn_ShouldReturnCorrectString()
    {
        // Group always appears between tab and card in the serialized string.
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings")
            .Card("Details")
            .Column("Left")
            .Group("search")
            .ToString();

        Assert.Equal("Content:5#Settings@search%Details|Left", result);
    }

    #endregion

    #region All Components Combined

    [Fact]
    public void Build_AllComponentsWithPositions_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1", "1")
            .Card("Card1", "2")
            .Column("Col1", "3", "9")
            .ToString();

        Assert.Equal("Content:5#Tab1;1%Card1;2|Col1_9;3", result);
    }

    [Fact]
    public void Build_LayoutZoneAllComponents_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .AsLayoutZone()
            .Tab("Tab1", "1")
            .Card("Card1", "2")
            .Column("Col1", "3", "9")
            .ToString();

        Assert.Equal("/Content:5#Tab1;1%Card1;2|Col1_9;3", result);
    }

    [Fact]
    public void Build_AllComponentsWithGroup_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1")
            .Group("Group1")
            .Card("Card1")
            .Column("Col1")
            .ToString();

        Assert.Equal("Content:5#Tab1@Group1%Card1|Col1", result);
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public void Build_SettingsTabPlacement_ShouldMatchManualString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parameters", "1")
            .Tab("Settings", "1")
            .ToString();

        Assert.Equal("Parameters:1#Settings;1", result);
    }

    [Fact]
    public void Build_CapabilitiesTabPlacement_ShouldMatchManualString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parameters", "5")
            .Tab("Capabilities", "5")
            .ToString();

        Assert.Equal("Parameters:5#Capabilities;5", result);
    }

    [Fact]
    public void Build_ColumnPlacementFromDocs_ShouldMatchManualString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parts", "0")
            .Column("Content", "1", "9")
            .ToString();

        Assert.Equal("Parts:0|Content_9;1", result);
    }

    [Fact]
    public void Build_HiddenLocation_ShouldUseStringDirectly()
    {
        Assert.Equal(PlacementInfo.HiddenLocation, "-");
    }

    #endregion

    #region PlacementInfo Parsing Roundtrip Tests

    [Fact]
    public void Build_ShouldProduceStringParsableByPlacementInfo_Zone()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
    }

    [Fact]
    public void Build_ShouldProduceStringParsableByPlacementInfo_Position()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5.1")
            .Tab("Tab1")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5.1", info.Position);
        Assert.Equal("Tab1", info.Tab.Name);
    }

    [Fact]
    public void Build_ShouldProduceStringParsableByPlacementInfo_AllComponents()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1", "1")
            .Group("Group1")
            .Card("Card1", "2")
            .Column("Col1", "3", "9")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
        Assert.Equal("Tab1", info.Tab.Name);
        Assert.Equal("1", info.Tab.Position);
        Assert.Equal("Group1", info.Group);
        Assert.Equal("Card1", info.Card.Name);
        Assert.Equal("2", info.Card.Position);
        Assert.Equal("Col1", info.Column.Name);
        Assert.Equal("3", info.Column.Position);
        Assert.Equal("9", info.Column.Width);
    }

    [Fact]
    public void Build_LayoutZone_ShouldBeDetectedByPlacementInfo()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .AsLayoutZone()
            .ToString();

        var info = new PlacementInfo(location);
        Assert.True(info.IsLayoutZone());
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
    }

    [Fact]
    public void Roundtrip_ZoneOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Parameters")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Parameters", info.Zones.First());
        Assert.Empty(info.Position);
        Assert.False(info.Tab.HasValue);
        Assert.Null(info.Group);
        Assert.False(info.Card.HasValue);
        Assert.False(info.Column.HasValue);
        Assert.False(info.IsLayoutZone());
    }

    [Fact]
    public void Roundtrip_ZoneWithPosition_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "3.5")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("3.5", info.Position);
    }

    [Fact]
    public void Roundtrip_ZoneWithBeforePosition_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "before")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("before", info.Position);
    }

    [Fact]
    public void Roundtrip_TabOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("1", info.Position);
        Assert.Equal("Settings", info.Tab.Name);
        Assert.False(info.Card.HasValue);
        Assert.False(info.Column.HasValue);
    }

    [Fact]
    public void Roundtrip_TabWithPosition_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings", "2")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Settings", info.Tab.Name);
        Assert.Equal("2", info.Tab.Position);
    }

    [Fact]
    public void Roundtrip_CardOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Card("Details", "3")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
        Assert.Equal("Details", info.Card.Name);
        Assert.Equal("3", info.Card.Position);
        Assert.False(info.Tab.HasValue);
        Assert.False(info.Column.HasValue);
    }

    [Fact]
    public void Roundtrip_ColumnOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Column("Left", "1", "9")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
        Assert.Equal("Left", info.Column.Name);
        Assert.Equal("1", info.Column.Position);
        Assert.Equal("9", info.Column.Width);
        Assert.False(info.Tab.HasValue);
        Assert.False(info.Card.HasValue);
    }

    [Fact]
    public void Roundtrip_ColumnWithBreakpointWidth_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Sidebar", "2", "lg-3")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Sidebar", info.Column.Name);
        Assert.Equal("2", info.Column.Position);
        Assert.Equal("lg-3", info.Column.Width);
    }

    [Fact]
    public void Roundtrip_GroupOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Group("search")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("search", info.Group);
        Assert.False(info.Tab.HasValue);
        Assert.False(info.Card.HasValue);
    }

    [Fact]
    public void Roundtrip_TabThenCard_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings", "1")
            .Card("Details", "2")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
        Assert.Equal("Settings", info.Tab.Name);
        Assert.Equal("1", info.Tab.Position);
        Assert.Equal("Details", info.Card.Name);
        Assert.Equal("2", info.Card.Position);
        Assert.False(info.Column.HasValue);
    }

    [Fact]
    public void Roundtrip_TabThenCardThenColumn_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings", "1")
            .Card("Details", "2")
            .Column("Left", "3", "6")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
        Assert.Equal("Settings", info.Tab.Name);
        Assert.Equal("1", info.Tab.Position);
        Assert.Equal("Details", info.Card.Name);
        Assert.Equal("2", info.Card.Position);
        Assert.Equal("Left", info.Column.Name);
        Assert.Equal("3", info.Column.Position);
        Assert.Equal("6", info.Column.Width);
    }

    [Fact]
    public void Roundtrip_CardThenColumn_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Parts", "0")
            .Card("Main")
            .Column("Right", "2", "3")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Parts", info.Zones.First());
        Assert.Equal("0", info.Position);
        Assert.False(info.Tab.HasValue);
        Assert.Equal("Main", info.Card.Name);
        Assert.Equal("Right", info.Column.Name);
        Assert.Equal("2", info.Column.Position);
        Assert.Equal("3", info.Column.Width);
    }

    [Fact]
    public void Roundtrip_TabWithGroup_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings")
            .Group("advanced")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.Equal("Settings", info.Tab.Name);
        Assert.Equal("advanced", info.Group);
    }

    [Fact]
    public void Roundtrip_LayoutZoneFullNesting_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .AsLayoutZone()
            .Tab("General", "1")
            .Card("Info", "2")
            .Column("Wide", "1", "12")
            .ToString();

        var info = new PlacementInfo(location);
        Assert.True(info.IsLayoutZone());
        Assert.Equal("Content", info.Zones.First());
        Assert.Equal("5", info.Position);
        Assert.Equal("General", info.Tab.Name);
        Assert.Equal("1", info.Tab.Position);
        Assert.Equal("Info", info.Card.Name);
        Assert.Equal("2", info.Card.Position);
        Assert.Equal("Wide", info.Column.Name);
        Assert.Equal("1", info.Column.Position);
        Assert.Equal("12", info.Column.Width);
    }

    [Fact]
    public void Roundtrip_DottedZone_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content.Metadata", "1")
            .ToString();

        var info = new PlacementInfo(location);
        var zones = info.Zones;
        Assert.Equal("Content", zones[0]);
        Assert.Equal("Metadata", zones[1]);
        Assert.Equal("1", info.Position);
    }

    #endregion

    #region Implicit Conversion and ToString Tests

    [Fact]
    public void ImplicitConversion_FromBuilder_AfterZone_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1");

        Assert.Equal("Content:1", result);
    }

    [Fact]
    public void ImplicitConversion_FromBuilder_AfterTab_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings", "1");

        Assert.Equal("Content:1#Settings;1", result);
    }

    [Fact]
    public void ImplicitConversion_FromBuilder_AfterCard_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings")
            .Card("Details", "2");

        Assert.Equal("Content:1#Settings%Details;2", result);
    }

    [Fact]
    public void ImplicitConversion_FromBuilder_AfterColumn_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Column("Left", "1", "9");

        Assert.Equal("Content:1|Left_9;1", result);
    }

    [Fact]
    public void ToString_AfterZone_ShouldReturnBuildResult()
    {
        var builder = new PlacementLocationBuilder()
            .Zone("Content", "1");

        Assert.Equal("Content:1", builder.ToString());
    }

    [Fact]
    public void ToString_AfterTab_ShouldReturnBuildResult()
    {
        var builder = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings");

        Assert.Equal("Content:1#Settings", builder.ToString());
    }

    #endregion

    #region Type Safety Tests - All Methods Return Same Builder

    [Fact]
    public void TypeSafety_ZoneReturnsSameBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var result = builder.Zone("Content");
        Assert.Same(builder, result);
    }

    [Fact]
    public void TypeSafety_TabReturnsSameBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var result = builder.Zone("Content").Tab("Settings");
        Assert.Same(builder, result);
    }

    [Fact]
    public void TypeSafety_CardReturnsSameBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var result = builder.Zone("Content").Card("Details");
        Assert.Same(builder, result);
    }

    [Fact]
    public void TypeSafety_ColumnReturnsSameBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var result = builder.Zone("Content").Column("Left");
        Assert.Same(builder, result);
    }

    [Fact]
    public void TypeSafety_GroupReturnsSameBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var result = builder.Zone("Content").Group("search");
        Assert.Same(builder, result);
    }

    [Fact]
    public void TypeSafety_AsLayoutZoneReturnsSameBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var result = builder.Zone("Content").AsLayoutZone();
        Assert.Same(builder, result);
    }

    #endregion
}
