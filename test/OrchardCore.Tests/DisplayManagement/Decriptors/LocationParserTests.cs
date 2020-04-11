using System.Linq;
using OrchardCore.DisplayManagement.Descriptors;
using Xunit;

namespace OrchardCore.Tests.DisplayManagement.Decriptors
{
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
        [InlineData("/Content:5@Group1")]
        [InlineData("/Content:5@Group1#Tab1")]
        public void ZoneShouldBeParsed(string location)
        {
            Assert.Equal("Content", new PlacementInfo { Location = location }.GetZones().FirstOrDefault());
        }

        [Theory]
        [InlineData("Content", "")]
        [InlineData("Content:5", "5")]
        [InlineData("Content:5#Tab1", "5")]
        [InlineData("Content:5.1#Tab1", "5.1")]
        [InlineData("Content:5@Group1", "5")]
        [InlineData("Content:5@Group1#Tab1", "5")]
        [InlineData("/Content", "")]
        [InlineData("/Content:5", "5")]
        [InlineData("/Content:5#Tab1", "5")]
        [InlineData("/Content:5.1#Tab1", "5.1")]
        [InlineData("/Content:5@Group1", "5")]
        [InlineData("/Content:5@Group1#Tab1", "5")]
        public void PositionShouldBeParsed(string location, string expectedPosition)
        {
            Assert.Equal(expectedPosition, new PlacementInfo { Location = location }.GetPosition());
        }

        [Theory]
        [InlineData("Content", false)]
        [InlineData("Content:5", false)]
        [InlineData("Content:5#Tab1", false)]
        [InlineData("Content:5.1#Tab1", false)]
        [InlineData("Content:5@Group1", false)]
        [InlineData("Content:5@Group1#Tab1", false)]
        [InlineData("/Content", true)]
        [InlineData("/Content:5", true)]
        [InlineData("/Content:5#Tab1", true)]
        [InlineData("/Content:5.1#Tab1", true)]
        [InlineData("/Content:5@Group1", true)]
        [InlineData("/Content:5@Group1#Tab1", true)]
        public void LayoutZoneShouldBeParsed(string location, bool expectedIsLayoutZone)
        {
            Assert.Equal(expectedIsLayoutZone, new PlacementInfo { Location = location }.IsLayoutZone());
        }

        [Theory]
        [InlineData("Content", "")]
        [InlineData("Content:5", "")]
        [InlineData("Content:5#Tab1", "Tab1")]
        [InlineData("Content:5.1#Tab1", "Tab1")]
        [InlineData("Content:5@Group1", "")]
        [InlineData("Content:5@Group1#Tab1", "Tab1")]
        [InlineData("Content:5#Tab1@Group1", "Tab1")]
        [InlineData("/Content", "")]
        [InlineData("/Content:5", "")]
        [InlineData("/Content:5#Tab1", "Tab1")]
        [InlineData("/Content:5.1#Tab1", "Tab1")]
        [InlineData("/Content:5@Group1", "")]
        [InlineData("/Content:5@Group1#Tab1", "Tab1")]
        [InlineData("/Content:5#Tab1@Group1", "Tab1")]
        public void TabShouldBeParsed(string location, string expectedTab)
        {
            Assert.Equal(expectedTab, new PlacementInfo { Location = location }.GetTab());
        }

        [Theory]
        [InlineData("Content", null)]
        [InlineData("Content:5", null)]
        [InlineData("Content:5#Tab1", null)]
        [InlineData("Content:5.1#Tab1", null)]
        [InlineData("Content:5@Group1", "Group1")]
        [InlineData("Content:5@Group1#Tab1", "Group1")]
        [InlineData("Content:5#Tab1@Group1", "Group1")]
        [InlineData("/Content", null)]
        [InlineData("/Content:5", null)]
        [InlineData("/Content:5#Tab1", null)]
        [InlineData("/Content:5.1#Tab1", null)]
        [InlineData("/Content:5@Group1", "Group1")]
        [InlineData("/Content:5@Group1#Tab1", "Group1")]
        [InlineData("/Content:5#Tab1@Group1", "Group1")]
        public void GroupShouldBeParsed(string location, string expectedGroup)
        {
            Assert.Equal(expectedGroup, new PlacementInfo { Location = location }.GetGroup());
        }
    }
}
