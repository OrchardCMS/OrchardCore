using System.Threading.Tasks;
using Assent;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Queries
{
    public class RecentBlogPostsQueryTests
    {
        [Fact]
        public async Task ShouldListBlogPostWhenCallingAQuery()
        {
            await BlogContext.InitializeBlogAsync();

            var blogPostContentItemId = await BlogContext
                .GraphQLClient
                .Content
                .Create("BlogPost", builder =>
                {
                    builder
                        .WithField("published", true);
                    builder
                        .WithField("latest", true);

                    builder
                        .WithContentPart("titlePart")
                        .AddField("title", "Some sorta blogpost in a Query!");

                    builder
                        .WithContentPart("containedPart")
                        .AddField("listContentItemId", BlogContext.BlogContentItemId);
                });

            var result = await BlogContext
                .GraphQLClient
                .Content
                .Query("RecentBlogPosts", builder =>
                {
                    builder
                        .WithNestedField("titlePart")
                        .AddField("title");
                });

            this.Assent(result.ToString());
        }
    }
}
