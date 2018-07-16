using System;
using System.Threading.Tasks;
using OrchardCore.Tests.Apis.JsonApi.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi
{
    public class BlogTests
    {
        private static Lazy<SiteContext> _siteContext;

        static BlogTests()
        {
            _siteContext = new Lazy<SiteContext>(() =>
            {
                var siteContext = new SiteContext();
                siteContext.InitializeAsync().GetAwaiter().GetResult();
                return siteContext;
            });
        }

        [Fact]
        public async Task ShouldCreateABlog() {
            var contentItemId = await _siteContext.Value
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
