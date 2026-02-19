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
            .Build();

        Assert.Equal("Content", result);
    }

    [Fact]
    public void Build_ZoneWithDotNotation_ShouldReturnDottedZone()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content.Metadata")
            .Build();

        Assert.Equal("Content.Metadata", result);
    }

    [Fact]
    public void Build_NoZone_ShouldThrowInvalidOperationException()
    {
        var builder = new PlacementLocationBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build());
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
            .Build();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Build_ZoneWithNullPosition_ShouldReturnZoneOnly()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", null)
            .Build();

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
            .Build();

        Assert.Equal("Content#Settings", result);
    }

    [Fact]
    public void Build_ZoneWithTabAndTabPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Tab("Settings", "1")
            .Build();

        Assert.Equal("Content#Settings;1", result);
    }

    [Fact]
    public void Build_ZonePositionAndTab_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parameters", "1")
            .Tab("Settings", "1")
            .Build();

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
            .Build();

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
            .Build();

        Assert.Equal("Content%Details", result);
    }

    [Fact]
    public void Build_ZoneWithCardAndCardPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Card("Details", "2")
            .Build();

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
            .Build();

        Assert.Equal("Content|Left", result);
    }

    [Fact]
    public void Build_ZoneWithColumnAndWidth_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", width: "9")
            .Build();

        Assert.Equal("Content|Left_9", result);
    }

    [Fact]
    public void Build_ZoneWithColumnPositionAndWidth_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", position: "1", width: "9")
            .Build();

        Assert.Equal("Content|Left_9;1", result);
    }

    [Fact]
    public void Build_ZoneWithColumnAndPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", position: "1")
            .Build();

        Assert.Equal("Content|Left;1", result);
    }

    [Fact]
    public void Build_ColumnWithBreakpointWidth_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left", width: "lg-9")
            .Build();

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
            .Build();

        Assert.Equal("/Content", result);
    }

    [Fact]
    public void Build_LayoutZoneWithPosition_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .AsLayoutZone()
            .Build();

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
            .Build();

        Assert.Equal("Content:5#Tab1;1%Card1;2", result);
    }

    [Fact]
    public void Build_TabThenColumn_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Tab1", "1")
            .Column("Col1", "2")
            .Build();

        Assert.Equal("Content:5#Tab1;1|Col1;2", result);
    }

    [Fact]
    public void Build_CardThenColumn_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Card("Card1", "1")
            .Column("Col1", "2", "9")
            .Build();

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
            .Build();

        Assert.Equal("Content:5#Tab1;1%Card1;2|Col1_9;3", result);
    }

    [Fact]
    public void Build_CardWithoutTab_ShouldWork()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Card("Details")
            .Build();

        Assert.Equal("Content:5%Details", result);
    }

    [Fact]
    public void Build_ColumnWithoutCardOrTab_ShouldWork()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Column("Left", "1", "6")
            .Build();

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
            .Build();

        Assert.Equal("Content:5@search", result);
    }

    [Fact]
    public void Build_GroupOnTab_ShouldReturnCorrectString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings")
            .Group("search")
            .Build();

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
            .Build();

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
            .Build();

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
            .Build();

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
            .Build();

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
            .Build();

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
            .Build();

        Assert.Equal("Parameters:1#Settings;1", result);
    }

    [Fact]
    public void Build_CapabilitiesTabPlacement_ShouldMatchManualString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parameters", "5")
            .Tab("Capabilities", "5")
            .Build();

        Assert.Equal("Parameters:5#Capabilities;5", result);
    }

    [Fact]
    public void Build_ColumnPlacementFromDocs_ShouldMatchManualString()
    {
        var result = new PlacementLocationBuilder()
            .Zone("Parts", "0")
            .Column("Content", "1", "9")
            .Build();

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
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
    }

    [Fact]
    public void Build_ShouldProduceStringParsableByPlacementInfo_Position()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5.1")
            .Tab("Tab1")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5.1", info.GetPosition());
        Assert.Equal("Tab1", info.GetTab());
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
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
        Assert.Equal("Tab1;1", info.GetTab());
        Assert.Equal("Group1", info.GetGroup());
        Assert.Equal("Card1;2", info.GetCard());
        Assert.Equal("Col1_9;3", info.GetColumn());
    }

    [Fact]
    public void Build_LayoutZone_ShouldBeDetectedByPlacementInfo()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .AsLayoutZone()
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.True(info.IsLayoutZone());
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
    }

    [Fact]
    public void Roundtrip_ZoneOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Parameters")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Parameters", info.GetZones().First());
        Assert.Empty(info.GetPosition());
        Assert.Empty(info.GetTab());
        Assert.Null(info.GetGroup());
        Assert.Null(info.GetCard());
        Assert.Null(info.GetColumn());
        Assert.False(info.IsLayoutZone());
    }

    [Fact]
    public void Roundtrip_ZoneWithPosition_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "3.5")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("3.5", info.GetPosition());
    }

    [Fact]
    public void Roundtrip_ZoneWithBeforePosition_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "before")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("before", info.GetPosition());
    }

    [Fact]
    public void Roundtrip_TabOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("1", info.GetPosition());
        Assert.Equal("Settings", info.GetTab());
        Assert.Null(info.GetCard());
        Assert.Null(info.GetColumn());
    }

    [Fact]
    public void Roundtrip_TabWithPosition_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings", "2")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Settings;2", info.GetTab());
    }

    [Fact]
    public void Roundtrip_CardOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Card("Details", "3")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
        Assert.Equal("Details;3", info.GetCard());
        Assert.Empty(info.GetTab());
        Assert.Null(info.GetColumn());
    }

    [Fact]
    public void Roundtrip_ColumnOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Column("Left", "1", "9")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
        Assert.Equal("Left_9;1", info.GetColumn());
        Assert.Empty(info.GetTab());
        Assert.Null(info.GetCard());
    }

    [Fact]
    public void Roundtrip_ColumnWithBreakpointWidth_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Sidebar", "2", "lg-3")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Sidebar_lg-3;2", info.GetColumn());
    }

    [Fact]
    public void Roundtrip_GroupOnly_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Group("search")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("search", info.GetGroup());
        Assert.Empty(info.GetTab());
        Assert.Null(info.GetCard());
    }

    [Fact]
    public void Roundtrip_TabThenCard_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings", "1")
            .Card("Details", "2")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
        Assert.Equal("Settings;1", info.GetTab());
        Assert.Equal("Details;2", info.GetCard());
        Assert.Null(info.GetColumn());
    }

    [Fact]
    public void Roundtrip_TabThenCardThenColumn_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings", "1")
            .Card("Details", "2")
            .Column("Left", "3", "6")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
        Assert.Equal("Settings;1", info.GetTab());
        Assert.Equal("Details;2", info.GetCard());
        Assert.Equal("Left_6;3", info.GetColumn());
    }

    [Fact]
    public void Roundtrip_CardThenColumn_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Parts", "0")
            .Card("Main")
            .Column("Right", "2", "3")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Parts", info.GetZones().First());
        Assert.Equal("0", info.GetPosition());
        Assert.Empty(info.GetTab());
        Assert.Equal("Main", info.GetCard());
        Assert.Equal("Right_3;2", info.GetColumn());
    }

    [Fact]
    public void Roundtrip_TabWithGroup_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content", "5")
            .Tab("Settings")
            .Group("advanced")
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.Equal("Settings", info.GetTab());
        Assert.Equal("advanced", info.GetGroup());
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
            .Build();

        var info = new PlacementInfo { Location = location };
        Assert.True(info.IsLayoutZone());
        Assert.Equal("Content", info.GetZones().First());
        Assert.Equal("5", info.GetPosition());
        Assert.Equal("General;1", info.GetTab());
        Assert.Equal("Info;2", info.GetCard());
        Assert.Equal("Wide_12;1", info.GetColumn());
    }

    [Fact]
    public void Roundtrip_DottedZone_ShouldParseCorrectly()
    {
        var location = new PlacementLocationBuilder()
            .Zone("Content.Metadata", "1")
            .Build();

        var info = new PlacementInfo { Location = location };
        var zones = info.GetZones();
        Assert.Equal("Content", zones[0]);
        Assert.Equal("Metadata", zones[1]);
        Assert.Equal("1", info.GetPosition());
    }

    #endregion

    #region Implicit Conversion and ToString Tests

    [Fact]
    public void ImplicitConversion_FromZoneBuilder_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1");

        Assert.Equal("Content:1", result);
    }

    [Fact]
    public void ImplicitConversion_FromTabBuilder_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings", "1");

        Assert.Equal("Content:1#Settings;1", result);
    }

    [Fact]
    public void ImplicitConversion_FromCardBuilder_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings")
            .Card("Details", "2");

        Assert.Equal("Content:1#Settings%Details;2", result);
    }

    [Fact]
    public void ImplicitConversion_FromColumnBuilder_ShouldReturnBuildResult()
    {
        string result = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Column("Left", "1", "9");

        Assert.Equal("Content:1|Left_9;1", result);
    }

    [Fact]
    public void ToString_OnZoneBuilder_ShouldReturnBuildResult()
    {
        var builder = new PlacementLocationBuilder()
            .Zone("Content", "1");

        Assert.Equal("Content:1", builder.ToString());
    }

    [Fact]
    public void ToString_OnTabBuilder_ShouldReturnBuildResult()
    {
        var builder = new PlacementLocationBuilder()
            .Zone("Content", "1")
            .Tab("Settings");

        Assert.Equal("Content:1#Settings", builder.ToString());
    }

    #endregion

    #region Type Safety Tests - Hierarchy Enforcement

    [Fact]
    public void TypeSafety_ZoneReturnsPlacementZoneBuilder()
    {
        var builder = new PlacementLocationBuilder();
        var zoneBuilder = builder.Zone("Content");
        Assert.IsType<PlacementZoneBuilder>(zoneBuilder);
    }

    [Fact]
    public void TypeSafety_TabReturnsPlacementTabBuilder()
    {
        var tabBuilder = new PlacementLocationBuilder()
            .Zone("Content")
            .Tab("Settings");
        Assert.IsType<PlacementTabBuilder>(tabBuilder);
    }

    [Fact]
    public void TypeSafety_CardFromZoneReturnsPlacementCardBuilder()
    {
        var cardBuilder = new PlacementLocationBuilder()
            .Zone("Content")
            .Card("Details");
        Assert.IsType<PlacementCardBuilder>(cardBuilder);
    }

    [Fact]
    public void TypeSafety_CardFromTabReturnsPlacementCardBuilder()
    {
        var cardBuilder = new PlacementLocationBuilder()
            .Zone("Content")
            .Tab("Settings")
            .Card("Details");
        Assert.IsType<PlacementCardBuilder>(cardBuilder);
    }

    [Fact]
    public void TypeSafety_ColumnFromCardReturnsPlacementColumnBuilder()
    {
        var columnBuilder = new PlacementLocationBuilder()
            .Zone("Content")
            .Card("Details")
            .Column("Left");
        Assert.IsType<PlacementColumnBuilder>(columnBuilder);
    }

    [Fact]
    public void TypeSafety_ColumnFromZoneReturnsPlacementColumnBuilder()
    {
        var columnBuilder = new PlacementLocationBuilder()
            .Zone("Content")
            .Column("Left");
        Assert.IsType<PlacementColumnBuilder>(columnBuilder);
    }

    #endregion
}
