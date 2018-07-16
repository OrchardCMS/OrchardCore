using System;
using System.Threading.Tasks;
using Assent;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class RecentBlogPostsQueryTests
    {
        private static Lazy<BlogContext> _context;

        static RecentBlogPostsQueryTests()
        {
            _context = new Lazy<BlogContext>(() =>
            {
                var siteContext = new BlogContext();
                siteContext.InitializeAsync().GetAwaiter().GetResult();
                return siteContext;
            });
        }

        [Fact(Skip = "Lucene Require rewriting")]
        public async Task ShouldListBlogPostWhenCallingAQuery()
        {
            var blogPostContentItemId = await _context.Value
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
                        .AddField("ListContentItemId", _context.Value.BlogContentItemId);
                });

            var result = await _context.Value
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
