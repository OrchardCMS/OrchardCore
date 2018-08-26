using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogTests
    {
        [Fact]
        public async Task ShouldCreateABlog() {

            await SiteContext.InitializeSiteAsync();

            var contentItemId = await SiteContext
                .GraphQLClient
                .Content
                .Create("Blog", builder => builder
                        .WithContentPart("titlePart")
                        .AddField("title", "Hi There!")
                );

            Assert.NotEmpty(contentItemId);
        }
    }
}
