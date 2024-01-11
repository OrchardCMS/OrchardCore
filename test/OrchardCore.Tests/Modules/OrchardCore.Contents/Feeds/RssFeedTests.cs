using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Contents.Feeds.Builders;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Feeds.Models;
using OrchardCore.Feeds.Rss;

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

            contentManagerMock.SetReturnsDefault(Task.FromResult(new ContentItemMetadata
            {
                DisplayRouteValues = new RouteValueDictionary()
            }));

            contentManagerMock.SetReturnsDefault(Task.FromResult(new BodyAspect
            {
                Body = new HtmlString("<p>The news description goes here ...</p>")
            }));

            feedContext.Builder.AddItem(feedContext, new ContentItem
            {
                DisplayText = "News",
                PublishedUtc = DateTime.UtcNow
            });

            // Act
            await commonFeedItemBuilder.PopulateAsync(feedContext);

            // Assert
            var description = feedContext.Response.Items[0].Element.Element("description").ToString();

            Assert.NotEqual("<description>&lt;![CDATA[&lt;p&gt;The news description goes here ...&lt;/p&gt;]]&gt;</description>", description);
            Assert.Equal("<description><![CDATA[<p>The news description goes here ...</p>]]></description>", description);
        }

        [Theory]
        [InlineData("rss")]
        [InlineData("non rss")]
        public async Task ShouldOnlyHtmlEntityEscapeFeedTitle(string format)
        {
            // Arrange
            var contentManagerMock = new Mock<IContentManager>();
            var commonFeedItemBuilder = new CommonFeedItemBuilder(contentManagerMock.Object);
            var feedContext = CreateFeedContext(format);

            contentManagerMock.SetReturnsDefault(Task.FromResult(new ContentItemMetadata
            {
                DisplayRouteValues = new RouteValueDictionary()
            }));

            contentManagerMock.SetReturnsDefault(Task.FromResult(new BodyAspect
            {
                Body = new HtmlString("<p>The news description goes here ...</p>")
            }));

            feedContext.Builder.AddItem(feedContext, new ContentItem
            {
                DisplayText = "It's a great title & so much > than anybody's!",
                PublishedUtc = DateTime.UtcNow
            });

            // Act
            await commonFeedItemBuilder.PopulateAsync(feedContext);

            // Assert
            var title = feedContext.Response.Items[0].Element.Element("title").ToString();

            // Test to ensure that double encoding of title does not occur and complies with XML requirements
            Assert.Equal("<title>It's a great title &amp; so much &gt; than anybody's!</title>", title);
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
