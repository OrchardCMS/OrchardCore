using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class RecentBlogPostsQueryTests
    {
        private BlogContext _context;

        public RecentBlogPostsQueryTests()
        {
            _context = new BlogContext();
        }

        [Fact]
        public async Task ShouldListBlogPostWhenCallingAQuery()
        {
            await _context.InitializeBlogAsync();

            var blogPostContentItemId = await BlogContext
                .GraphQLClient
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
                        .AddField("ListContentItemId", BlogContext.BlogContentItemId);
                });

            var result = await BlogContext
                .GraphQLClient
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
