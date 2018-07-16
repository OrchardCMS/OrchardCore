using System.Threading.Tasks;
using Assent;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class RecentBlogPostsQueryTests
    {
        private static BlogContext _context;
        private static object _sync = new object();

        static RecentBlogPostsQueryTests()
        {
        }

        [Fact(Skip = "Lucene Require rewriting")]
        public async Task ShouldListBlogPostWhenCallingAQuery()
        {
            if (_context == null)
            {
                lock (_sync)
                {
                    if (_context == null)
                    {
                        var context = new BlogContext();
                        context.InitializeAsync().GetAwaiter().GetResult();
                        _context = context;
                    }
                }
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
