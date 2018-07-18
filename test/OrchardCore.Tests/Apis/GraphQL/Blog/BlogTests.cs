using System.Threading.Tasks;
using OrchardCore.Tests.Apis.GraphQL.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogTests
    {
        private static SiteContext _siteContext;
        private static object _sync = new object();
        private static bool _initialize;

        public BlogTests()
        {
        }

        [Fact(Skip = "For testing")]
        public async Task ShouldCreateABlog() {

            var initialize = false;

            if (!_initialize)
            {
                lock (_sync)
                {
                    if (!_initialize)
                    {

                        initialize = true;
                        _initialize = true;
                    }
                }
            }

            if (initialize)
            {
                var context = new SiteContext();
                context.Initialize();
                await context.InitializeAsync();
                _siteContext = context;
            }

            while (_siteContext == null)
            {
                await Task.Delay(5000);
            }

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
