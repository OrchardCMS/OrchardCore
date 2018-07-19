using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi
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
                .JsonApiClient                
                .Content
                .Create("Blog", builder => builder
                        .WithContentPart("TitlePart", partBuilder => partBuilder
                            .WithProperty("Title", "Hi There!"))
                );

            Assert.NotEmpty(contentItemId);
        }
    }
}
