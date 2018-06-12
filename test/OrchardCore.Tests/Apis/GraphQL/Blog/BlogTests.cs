using System.Threading.Tasks;
using OrchardCore.Tests.Apis.GraphQL.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogTests : IClassFixture<SiteContext>
    {
        private SiteContext _siteContext;

        public BlogTests(SiteContext siteContext)
        {
            _siteContext = siteContext;
        }

        [Fact]
        public async Task ShouldCreateABlog() {
            var contentItemId = await _siteContext
                .Client
                .Content
                .Create("Blog", builder => builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Hi There!")
                );

            Assert.NotEmpty(contentItemId);
        }
    }
}
