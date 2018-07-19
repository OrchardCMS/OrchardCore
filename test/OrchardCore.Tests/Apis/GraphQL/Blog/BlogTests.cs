using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogTests
    {
        private SiteContext _context;

        public BlogTests()
        {
            _context = new SiteContext();
        }

        [Fact]
        public async Task ShouldCreateABlog() {

            await _context.InitializeSiteAsync();

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
