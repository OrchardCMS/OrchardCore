using Orchard.DisplayManagement.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Orchard.Tests.DisplayManagement.Decriptors
{
    public class LocationParserTests
    {
        [Fact]
        public void ZoneShouldBeParsed()
        {
            Assert.Equal("Content", new PlacementInfo { Location = "/Content" }.GetZone());
            Assert.Equal("Content", new PlacementInfo { Location = "Content" }.GetZone());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5" }.GetZone());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5#Tab1" }.GetZone());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5@Group1" }.GetZone());
            Assert.Equal("Content", new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetZone());
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
            Assert.Equal(true, new PlacementInfo { Location = "/Content" }.IsLayoutZone());
            Assert.Equal(true, new PlacementInfo { Location = "/Content:5" }.IsLayoutZone());
            Assert.Equal(false, new PlacementInfo { Location = "Content:5#Tab1" }.IsLayoutZone());
            Assert.Equal(false, new PlacementInfo { Location = "Content:5.1#Tab1" }.IsLayoutZone());
            Assert.Equal(false, new PlacementInfo { Location = "Content:5@Group1" }.IsLayoutZone());
            Assert.Equal(false, new PlacementInfo { Location = "Content:5@Group1#Tab1" }.IsLayoutZone());
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
            Assert.Equal("", new PlacementInfo { Location = "Content" }.GetGroup());
            Assert.Equal("", new PlacementInfo { Location = "Content:5" }.GetGroup());
            Assert.Equal("", new PlacementInfo { Location = "Content:5#Tab1" }.GetGroup());
            Assert.Equal("", new PlacementInfo { Location = "Content:5.1#Tab1" }.GetGroup());
            Assert.Equal("Group1", new PlacementInfo { Location = "Content:5@Group1" }.GetGroup());
            Assert.Equal("Group1", new PlacementInfo { Location = "Content:5@Group1#Tab1" }.GetGroup());
            Assert.Equal("Group1", new PlacementInfo { Location = "Content:5#Tab1@Group1" }.GetGroup());
        }
    }
}