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
    [InlineData("/Content:5#Tab1|Col1", "Tab1")]
    [InlineData("/Content:5.1#Tab1|Col1", "Tab1")]
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
        var info = new PlacementInfo(location);
        var actualTab = info.Tab.HasValue ? info.Tab.Name : "";
        Assert.Equal(expectedTab, actualTab);
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
        var info = new PlacementInfo(location);
        var actualColumn = info.Column.HasValue ? info.Column.Name : null;
        Assert.Equal(expectedColumn, actualColumn);
    }

    [Theory]
    [InlineData("Content", null)]
    [InlineData("Content:5", null)]
    [InlineData("Content:5#Tab1", null)]
    [InlineData("Content:5@Group1", null)]
    [InlineData("Content:5|Col1", null)]
    [InlineData("Content:5%Card1", "Card1")]
    [InlineData("Content:5%Card1;3", "Card1")]
    [InlineData("Content:5#Tab1%Card1", "Card1")]
    [InlineData("Content:5#Tab1%Card1;2", "Card1")]
    [InlineData("Content:5@Group1%Card1", "Card1")]
    [InlineData("Content:5@Group1%Card1;5", "Card1")]
    [InlineData("Content:5#Tab1@Group1%Card1", "Card1")]
    [InlineData("Content:5#Tab1@Group1%Card1;1", "Card1")]
    [InlineData("Content:5%Card1|Col1", "Card1")]
    [InlineData("Content:5%Card1;3|Col1", "Card1")]
    [InlineData("Content:5#Tab1%Card1|Col1", "Card1")]
    [InlineData("Content:5@Group1%Card1|Col1", "Card1")]
    [InlineData("Content:5#Tab1@Group1%Card1|Col1", "Card1")]
    [InlineData("/Content", null)]
    [InlineData("/Content:5", null)]
    [InlineData("/Content:5#Tab1", null)]
    [InlineData("/Content:5@Group1", null)]
    [InlineData("/Content:5|Col1", null)]
    [InlineData("/Content:5%Card1", "Card1")]
    [InlineData("/Content:5%Card1;3", "Card1")]
    [InlineData("/Content:5#Tab1%Card1", "Card1")]
    [InlineData("/Content:5#Tab1%Card1;2", "Card1")]
    [InlineData("/Content:5@Group1%Card1", "Card1")]
    [InlineData("/Content:5@Group1%Card1;5", "Card1")]
    [InlineData("/Content:5#Tab1@Group1%Card1", "Card1")]
    [InlineData("/Content:5#Tab1@Group1%Card1;1", "Card1")]
    [InlineData("/Content:5%Card1|Col1", "Card1")]
    [InlineData("/Content:5%Card1;3|Col1", "Card1")]
    [InlineData("/Content:5#Tab1%Card1|Col1", "Card1")]
    [InlineData("/Content:5@Group1%Card1|Col1", "Card1")]
    [InlineData("/Content:5#Tab1@Group1%Card1|Col1", "Card1")]
    public void CardNameShouldBeParsed(string location, string expectedCardName)
    {
        var info = new PlacementInfo(location);
        var actualCardName = info.Card.HasValue ? info.Card.Name : null;
        Assert.Equal(expectedCardName, actualCardName);
    }

    [Theory]
    [InlineData("Content:5%Card1", null)]
    [InlineData("Content:5%Card1;3", "3")]
    [InlineData("Content:5%Card1;1.5", "1.5")]
    [InlineData("Content:5%Card1;before", "before")]
    [InlineData("Content:5%Card1;after", "after")]
    [InlineData("Content:5#Tab1%Card1;2", "2")]
    [InlineData("Content:5@Group1%Card1;5", "5")]
    [InlineData("Content:5#Tab1@Group1%Card1;1", "1")]
    [InlineData("Content:5%Card1;3|Col1", "3")]
    [InlineData("Content:5#Tab1%Card1;2|Col1", "2")]
    [InlineData("/Content:5%Card1", null)]
    [InlineData("/Content:5%Card1;3", "3")]
    [InlineData("/Content:5%Card1;1.5", "1.5")]
    [InlineData("/Content:5%Card1;before", "before")]
    [InlineData("/Content:5%Card1;after", "after")]
    [InlineData("/Content:5#Tab1%Card1;2", "2")]
    [InlineData("/Content:5@Group1%Card1;5", "5")]
    [InlineData("/Content:5#Tab1@Group1%Card1;1", "1")]
    [InlineData("/Content:5%Card1;3|Col1", "3")]
    [InlineData("/Content:5#Tab1%Card1;2|Col1", "2")]
    public void CardPositionShouldBeParsed(string location, string expectedCardPosition)
    {
        var info = new PlacementInfo(location);
        var actualCardPosition = info.Card.HasValue ? info.Card.Position : null;
        Assert.Equal(expectedCardPosition, actualCardPosition);
    }

    [Theory]
    [InlineData("Content:5#Tab1", null)]
    [InlineData("Content:5#Tab1;3", "3")]
    [InlineData("Content:5#Tab1;1.5", "1.5")]
    [InlineData("Content:5#Tab1;before", "before")]
    [InlineData("Content:5#Tab1;after", "after")]
    [InlineData("Content:5#Tab1;2@Group1", "2")]
    [InlineData("Content:5#Tab1;3%Card1", "3")]
    [InlineData("Content:5#Tab1;4|Col1", "4")]
    [InlineData("Content:5#Tab1;5@Group1%Card1", "5")]
    [InlineData("Content:5#Tab1;6@Group1%Card1|Col1", "6")]
    [InlineData("/Content:5#Tab1", null)]
    [InlineData("/Content:5#Tab1;3", "3")]
    [InlineData("/Content:5#Tab1;1.5", "1.5")]
    [InlineData("/Content:5#Tab1;before", "before")]
    [InlineData("/Content:5#Tab1;after", "after")]
    [InlineData("/Content:5#Tab1;2@Group1", "2")]
    [InlineData("/Content:5#Tab1;3%Card1", "3")]
    [InlineData("/Content:5#Tab1;4|Col1", "4")]
    [InlineData("/Content:5#Tab1;5@Group1%Card1", "5")]
    [InlineData("/Content:5#Tab1;6@Group1%Card1|Col1", "6")]
    public void TabPositionShouldBeParsed(string location, string expectedTabPosition)
    {
        var info = new PlacementInfo(location);
        var actualTabPosition = info.Tab.HasValue ? info.Tab.Position : null;
        Assert.Equal(expectedTabPosition, actualTabPosition);
    }

    [Theory]
    [InlineData("Content:5|Col1", null)]
    [InlineData("Content:5|Col1_9", "9")]
    [InlineData("Content:5|Col1_3", "3")]
    [InlineData("Content:5|Col1_lg-6", "lg-6")]
    [InlineData("Content:5|Col1_md-4", "md-4")]
    [InlineData("Content:5|Col1_sm-12", "sm-12")]
    [InlineData("Content:5#Tab1|Col1_9", "9")]
    [InlineData("Content:5@Group1|Col1_6", "6")]
    [InlineData("Content:5%Card1|Col1_3", "3")]
    [InlineData("Content:5#Tab1@Group1|Col1_9", "9")]
    [InlineData("Content:5#Tab1%Card1|Col1_6", "6")]
    [InlineData("Content:5@Group1%Card1|Col1_3", "3")]
    [InlineData("Content:5#Tab1@Group1%Card1|Col1_9", "9")]
    [InlineData("/Content:5|Col1", null)]
    [InlineData("/Content:5|Col1_9", "9")]
    [InlineData("/Content:5|Col1_3", "3")]
    [InlineData("/Content:5|Col1_lg-6", "lg-6")]
    [InlineData("/Content:5|Col1_md-4", "md-4")]
    [InlineData("/Content:5|Col1_sm-12", "sm-12")]
    [InlineData("/Content:5#Tab1|Col1_9", "9")]
    [InlineData("/Content:5@Group1|Col1_6", "6")]
    [InlineData("/Content:5%Card1|Col1_3", "3")]
    [InlineData("/Content:5#Tab1@Group1|Col1_9", "9")]
    [InlineData("/Content:5#Tab1%Card1|Col1_6", "6")]
    [InlineData("/Content:5@Group1%Card1|Col1_3", "3")]
    [InlineData("/Content:5#Tab1@Group1%Card1|Col1_9", "9")]
    public void ColumnWidthShouldBeParsed(string location, string expectedColumnWidth)
    {
        var info = new PlacementInfo(location);
        var actualColumnWidth = info.Column.HasValue ? info.Column.Width : null;
        Assert.Equal(expectedColumnWidth, actualColumnWidth);
    }

    [Theory]
    [InlineData("Content:5|Col1", null)]
    [InlineData("Content:5|Col1;2", "2")]
    [InlineData("Content:5|Col1;1.5", "1.5")]
    [InlineData("Content:5|Col1;before", "before")]
    [InlineData("Content:5|Col1;after", "after")]
    [InlineData("Content:5|Col1_9;2", "2")]
    [InlineData("Content:5|Col1;2_9", "2")]
    [InlineData("Content:5#Tab1|Col1;3", "3")]
    [InlineData("Content:5@Group1|Col1;4", "4")]
    [InlineData("Content:5%Card1|Col1;5", "5")]
    [InlineData("Content:5#Tab1@Group1|Col1;6", "6")]
    [InlineData("Content:5#Tab1%Card1|Col1;7", "7")]
    [InlineData("Content:5@Group1%Card1|Col1;8", "8")]
    [InlineData("Content:5#Tab1@Group1%Card1|Col1;9", "9")]
    [InlineData("/Content:5|Col1", null)]
    [InlineData("/Content:5|Col1;2", "2")]
    [InlineData("/Content:5|Col1;1.5", "1.5")]
    [InlineData("/Content:5|Col1;before", "before")]
    [InlineData("/Content:5|Col1;after", "after")]
    [InlineData("/Content:5|Col1_9;2", "2")]
    [InlineData("/Content:5|Col1;2_9", "2")]
    [InlineData("/Content:5#Tab1|Col1;3", "3")]
    [InlineData("/Content:5@Group1|Col1;4", "4")]
    [InlineData("/Content:5%Card1|Col1;5", "5")]
    [InlineData("/Content:5#Tab1@Group1|Col1;6", "6")]
    [InlineData("/Content:5#Tab1%Card1|Col1;7", "7")]
    [InlineData("/Content:5@Group1%Card1|Col1;8", "8")]
    [InlineData("/Content:5#Tab1@Group1%Card1|Col1;9", "9")]
    public void ColumnPositionShouldBeParsed(string location, string expectedColumnPosition)
    {
        var info = new PlacementInfo(location);
        var actualColumnPosition = info.Column.HasValue ? info.Column.Position : null;
        Assert.Equal(expectedColumnPosition, actualColumnPosition);
    }

    [Theory]
    [InlineData("Content:5|Col1_9;2", "Col1", "9", "2")]
    [InlineData("Content:5|Col1;2_9", "Col1", "9", "2")]
    [InlineData("Content:5|Left_lg-6;1", "Left", "lg-6", "1")]
    [InlineData("Content:5|Right;3_md-4", "Right", "md-4", "3")]
    [InlineData("/Content:5|Col1_9;2", "Col1", "9", "2")]
    [InlineData("/Content:5|Col1;2_9", "Col1", "9", "2")]
    [InlineData("/Content:5|Left_lg-6;1", "Left", "lg-6", "1")]
    [InlineData("/Content:5|Right;3_md-4", "Right", "md-4", "3")]
    [InlineData("Content:5#Tab1@Group1%Card1|Col1_9;2", "Col1", "9", "2")]
    [InlineData("/Content:5#Tab1@Group1%Card1|Col1_9;2", "Col1", "9", "2")]
    public void ColumnWidthAndPositionShouldBeParsed(string location, string expectedName, string expectedWidth, string expectedPosition)
    {
        var info = new PlacementInfo(location);
        Assert.True(info.Column.HasValue);
        Assert.Equal(expectedName, info.Column.Name);
        Assert.Equal(expectedWidth, info.Column.Width);
        Assert.Equal(expectedPosition, info.Column.Position);
    }

    [Theory]
    [InlineData("Content", new[] { "Content" })]
    [InlineData("Content.Metadata", new[] { "Content", "Metadata" })]
    [InlineData("Content.Metadata.Details", new[] { "Content", "Metadata", "Details" })]
    [InlineData("Content:5", new[] { "Content" })]
    [InlineData("Content.Metadata:5", new[] { "Content", "Metadata" })]
    [InlineData("Content.Metadata.Details:5", new[] { "Content", "Metadata", "Details" })]
    [InlineData("Content.Metadata:5#Tab1", new[] { "Content", "Metadata" })]
    [InlineData("Content.Metadata:5@Group1", new[] { "Content", "Metadata" })]
    [InlineData("Content.Metadata:5%Card1", new[] { "Content", "Metadata" })]
    [InlineData("Content.Metadata:5|Col1", new[] { "Content", "Metadata" })]
    [InlineData("/Content", new[] { "Content" })]
    [InlineData("/Content.Metadata", new[] { "Content", "Metadata" })]
    [InlineData("/Content.Metadata.Details", new[] { "Content", "Metadata", "Details" })]
    [InlineData("/Content:5", new[] { "Content" })]
    [InlineData("/Content.Metadata:5", new[] { "Content", "Metadata" })]
    [InlineData("/Content.Metadata.Details:5", new[] { "Content", "Metadata", "Details" })]
    [InlineData("/Content.Metadata:5#Tab1@Group1%Card1|Col1", new[] { "Content", "Metadata" })]
    public void MultipleZonesShouldBeParsed(string location, string[] expectedZones)
    {
        var info = new PlacementInfo(location);
        Assert.Equal(expectedZones, info.Zones);
    }

    [Theory]
    [InlineData("Content:5#Tab1;2@Group1%Card1;3|Col1_9;4", "Content", "5", "Tab1", "2", "Group1", "Card1", "3", "Col1", "9", "4")]
    [InlineData("/Content:5#Tab1;2@Group1%Card1;3|Col1_9;4", "Content", "5", "Tab1", "2", "Group1", "Card1", "3", "Col1", "9", "4")]
    [InlineData("Content.Metadata:10#Settings;1@Advanced%Options;5|Left_lg-6;2", "Content", "10", "Settings", "1", "Advanced", "Options", "5", "Left", "lg-6", "2")]
    public void CompleteLocationStringShouldBeParsed(
        string location,
        string expectedZone,
        string expectedPosition,
        string expectedTabName,
        string expectedTabPosition,
        string expectedGroup,
        string expectedCardName,
        string expectedCardPosition,
        string expectedColumnName,
        string expectedColumnWidth,
        string expectedColumnPosition)
    {
        var info = new PlacementInfo(location);

        Assert.Equal(expectedZone, info.Zones.FirstOrDefault());
        Assert.Equal(expectedPosition, info.Position);
        Assert.Equal(expectedTabName, info.Tab.Name);
        Assert.Equal(expectedTabPosition, info.Tab.Position);
        Assert.Equal(expectedGroup, info.Group);
        Assert.Equal(expectedCardName, info.Card.Name);
        Assert.Equal(expectedCardPosition, info.Card.Position);
        Assert.Equal(expectedColumnName, info.Column.Name);
        Assert.Equal(expectedColumnWidth, info.Column.Width);
        Assert.Equal(expectedColumnPosition, info.Column.Position);
    }

    [Theory]
    [InlineData("-")]
    public void HiddenLocationShouldBeDetected(string location)
    {
        var info = new PlacementInfo(location);
        Assert.True(info.IsHidden());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptyOrNullLocationShouldBeHidden(string location)
    {
        var info = new PlacementInfo(location);
        Assert.True(info.IsHidden());
        Assert.Empty(info.Zones);
    }

    [Fact]
    public void FromLocation_ShouldReturnHiddenInstance_ForHiddenLocation()
    {
        var result = PlacementInfo.FromLocation("-");

        Assert.Same(PlacementInfo.Hidden, result);
    }

    [Fact]
    public void FromLocation_ShouldReturnNull_ForNullOrEmptyLocation()
    {
        Assert.Null(PlacementInfo.FromLocation(null));
        Assert.Null(PlacementInfo.FromLocation(""));
    }

    [Fact]
    public void FromLocation_ShouldReturnCachedInstance_ForSameLocation()
    {
        var first = PlacementInfo.FromLocation("Content:5#Tab1");
        var second = PlacementInfo.FromLocation("Content:5#Tab1");

        Assert.Same(first, second);
    }

    [Fact]
    public void WithSource_ShouldReturnSameInstance_WhenSourceUnchanged()
    {
        var placement = new PlacementInfo("Content:5", source: "TestSource");

        var result = placement.WithSource("TestSource");

        Assert.Same(placement, result);
    }

    [Fact]
    public void WithSource_ShouldReturnNewInstance_WhenSourceChanged()
    {
        var placement = new PlacementInfo("Content:5", source: "OldSource");

        var result = placement.WithSource("NewSource");

        Assert.NotSame(placement, result);
        Assert.Equal("NewSource", result.Source);
        Assert.Equal("Content:5", result.Location);
        Assert.Equal("5", result.Position);
    }

    [Fact]
    public void ToString_ShouldReturnLocation()
    {
        var placement = new PlacementInfo("Content:5#Tab1");

        Assert.Equal("Content:5#Tab1", placement.ToString());
    }

    [Fact]
    public void ToString_ShouldReturnEmpty_ForEmptyPlacement()
    {
        var placement = new PlacementInfo();

        Assert.Equal("(empty)", placement.ToString());
    }
}
