using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Routing;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Contents.Feeds.Builders;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Feeds.Models;
using OrchardCore.Feeds.Rss;
using Xunit;

namespace OrchardCore.Tests.Modules.Contents.Feeds
{
    public class RssFeedTests
    {
        [Theory]
        [InlineData("rss")]
        [InlineData("non rss")]
        public async Task AvoidDoubleEncodeCDATA(string format)
        {
            // Arrange
            var contentManagerMock = new Mock<IContentManager>();
            var commonFeedItemBuilder = new CommonFeedItemBuilder(contentManagerMock.Object);
            var feedContext = CreateFeedContext(format);
            var bodyContent = "<p>The news description goes here ...</p>";

            contentManagerMock.SetReturnsDefault(Task.FromResult(new ContentItemMetadata
            {
                DisplayRouteValues = new RouteValueDictionary()
            }));

            contentManagerMock.SetReturnsDefault(Task.FromResult(new BodyAspect
            {
                Body = new HtmlString(bodyContent)
            }));

            feedContext.Builder.AddItem(feedContext, new ContentItem
            {
                DisplayText = "News",
                PublishedUtc = DateTime.UtcNow
            });

            // Act
            await commonFeedItemBuilder.PopulateAsync(feedContext);

            // Assert
            Assert.Contains($"<![CDATA[{bodyContent}]]>", feedContext.Response.Items[0].Element.ToString());
        }

        private static FeedContext CreateFeedContext(string format)
        {
            var feedContextMock = new Mock<FeedContext>(Mock.Of<IUpdateModel>(), format);

            feedContextMock.Object.Response.Element = new XElement("Feed");
            feedContextMock.Object.Builder = new RssFeedBuilder();

            return feedContextMock.Object;
        }
    }
}
