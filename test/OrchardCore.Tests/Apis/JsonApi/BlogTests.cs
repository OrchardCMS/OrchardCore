using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi
{
    public class BlogTests
    {
        [Fact]
        public async Task ShouldCreateABlog() {

            await SiteContext.InitializeSiteAsync();

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
