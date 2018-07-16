using System;
using System.Threading.Tasks;
using OrchardCore.Tests.Apis.GraphQL.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
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
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Hi There!")
                );

            Assert.NotEmpty(contentItemId);
        }
    }
}
