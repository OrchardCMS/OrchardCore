using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Xunit;

namespace Orchard.Tests.DisplayManagement.Decriptors
{
    public class LocationParserTests
    {
        [Fact]
        public void ZoneShouldBeParsed()
        {
            Assert.Equal("Content", new PlacementInfo { Location = "/Content" }.GetZones().FirstOrDefault());
            Assert.Equal("Content", new PlacementInfo { Location = "Content" }.GetZones().FirstOrDefault());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5" }.GetZones().FirstOrDefault());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5#Tab1" }.GetZones().FirstOrDefault());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5@Group1" }.GetZones().FirstOrDefault());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetZones().FirstOrDefault());
        }

        [Fact]
        public void PositionShouldBeParsed()
        {
            Assert.Equal("", new PlacementInfo { Location = "Content" }.GetPosition());
            Assert.Equal("5", new PlacementInfo { Location = "Content:5" }.GetPosition());
            Assert.Equal("5", new PlacementInfo { Location = "Content:5#Tab1" }.GetPosition());
            Assert.Equal("5.1", new PlacementInfo { Location = "Content:5.1#Tab1" }.GetPosition());
            Assert.Equal("5", new PlacementInfo { Location = "Content:5@Group1" }.GetPosition());
            Assert.Equal("5", new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetPosition());
        }

        [Fact]
        public void LayoutZoneShouldBeParsed()
        {
            Assert.True(new PlacementInfo { Location = "/Content" }.IsLayoutZone());
            Assert.True(new PlacementInfo { Location = "/Content:5" }.IsLayoutZone());
            Assert.False(new PlacementInfo { Location = "Content:5#Tab1" }.IsLayoutZone());
            Assert.False(new PlacementInfo { Location = "Content:5.1#Tab1" }.IsLayoutZone());
            Assert.False(new PlacementInfo { Location = "Content:5@Group1" }.IsLayoutZone());
            Assert.False(new PlacementInfo { Location = "Content:5@Group1#Tab1" }.IsLayoutZone());
        }

        [Fact]
        public void TabShouldBeParsed()
        {
            Assert.Equal("", new PlacementInfo { Location = "Content" }.GetTab());
            Assert.Equal("", new PlacementInfo { Location = "Content:5" }.GetTab());
            Assert.Equal("Tab1", new PlacementInfo { Location = "Content:5#Tab1" }.GetTab());
            Assert.Equal("Tab1", new PlacementInfo { Location = "Content:5.1#Tab1" }.GetTab());
            Assert.Equal("", new PlacementInfo { Location = "Content:5@Group1" }.GetTab());
            Assert.Equal("Tab1", new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetTab());
            Assert.Equal("Tab1", new PlacementInfo { Location = "Content:5#Tab1@Group1" }.GetTab());
        }

        [Fact]
        public void GroupShouldBeParsed()
        {
            Assert.Null(new PlacementInfo { Location = "Content" }.GetGroup());
            Assert.Null(new PlacementInfo { Location = "Content:5" }.GetGroup());
            Assert.Null(new PlacementInfo { Location = "Content:5#Tab1" }.GetGroup());
            Assert.Null(new PlacementInfo { Location = "Content:5.1#Tab1" }.GetGroup());
            Assert.Equal("Group1", new PlacementInfo { Location = "Content:5@Group1" }.GetGroup());
            Assert.Equal("Group1", new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetGroup());
            Assert.Equal("Group1", new PlacementInfo { Location = "Content:5#Tab1@Group1" }.GetGroup());
        }
    }
}