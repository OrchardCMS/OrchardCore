using System.Threading.Tasks;
using OrchardCore.Tests.Apis.JsonApi.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi
{
    public class BlogTests
    {
        private static SiteContext _siteContext;
        private static object _sync = new object();

        public BlogTests()
        {
        }

        [Fact]
        public async Task ShouldCreateABlog() {

            if (_siteContext == null)
            {
                lock (_sync)
                {
                    if (_siteContext == null)
                    {
                        var context = new SiteContext();
                        context.InitializeAsync().GetAwaiter().GetResult();
                        _siteContext = context;
                    }
                }
            }

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
