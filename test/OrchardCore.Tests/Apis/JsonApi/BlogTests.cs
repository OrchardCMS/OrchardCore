using System.Threading.Tasks;
using OrchardCore.Tests.Apis.JsonApi.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi
{
    public class BlogTests // : IClassFixture<SiteContext>
    {
        private static SiteContext _siteContext;

        static BlogTests(/*SiteContext siteContext*/)
        {
            _siteContext = new SiteContext(); // siteContext;
            _siteContext.InitializeAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task ShouldCreateABlog() {
            var contentItemId = await _siteContext
                .Client
                .Content
                .Create("Blog", builder => builder
                        .WithContentPart("TitlePart", partBuilder => partBuilder
                            .WithProperty("Title", "Hi There!"))
                );

            Assert.NotEmpty(contentItemId);
        }
    }
}
