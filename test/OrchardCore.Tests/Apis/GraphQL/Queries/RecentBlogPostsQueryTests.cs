using System.Threading.Tasks;
using Assent;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class RecentBlogPostsQueryTests
    {
        private static BlogContext _context;
        private static object _sync = new object();
        private static bool _initialize;

        static RecentBlogPostsQueryTests()
        {
        }

        [Fact(Skip = "Lucene Require rewriting")]
        public async Task ShouldListBlogPostWhenCallingAQuery()
        {
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
                var context = new BlogContext();
                await context.InitializeAsync();
                _context = context;
            }

            while (_context == null)
            {
                await Task.Delay(5000);
            }

            var blogPostContentItemId = await _context
                .Client
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithField("Published", true);
                    builder
                        .WithField("Latest", true);

                    builder
                        .WithContentPart("TitlePart")
                        .AddField("Title", "Some sorta blogpost in a Query!");

                    builder
                        .WithContentPart("ContainedPart")
                        .AddField("ListContentItemId", _context.BlogContentItemId);
                });

            var result = await _context
                .Client
                .Content
                .Query("RecentBlogPosts", builder =>
                {
                    builder
                        .WithNestedField("TitlePart")
                        .AddField("Title");
                });

            this.Assent(result.ToString());
        }
    }
}
