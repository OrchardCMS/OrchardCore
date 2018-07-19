using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogTests
    {
        public BlogTests()
        {
            new SiteContext().InitializeSiteAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task ShouldCreateABlog() {
            var contentItemId = await SiteContext
                .GraphQLClient
                .Content
                .Create("Blog", builder => builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Hi There!")
                );

            Assert.NotEmpty(contentItemId);
        }
    }
}
