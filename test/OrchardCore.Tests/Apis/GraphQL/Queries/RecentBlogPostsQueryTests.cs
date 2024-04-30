using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Utilities;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class RecentBlogPostsQueryTests
    {
        [Fact]
        public async Task ShouldListBlogPostWhenCallingAQuery()
        {
            using var context = new BlogContext();
            await context.InitializeAsync();

            var blogPostContentItemId = await context
                .CreateContentItem("BlogPost", builder =>
                {
                    builder.Published = true;
                    builder.Latest = true;
                    builder.DisplayText = "Some sorta blogpost in a Query!";

                    builder
                        .Weld(new ContainedPart
                        {
                            ListContentItemId = context.BlogContentItemId
                        });
                });

            // Search indexes are no longer updated in a deferred task at the end of a shell scope
            // but in a background job after the http request, so they are not already up to date.
            await TimeoutTaskRunner.RunAsync(TimeSpan.FromSeconds(5), async () =>
            {
                var result = await context
                    .GraphQLClient
                    .Content
                    .Query("RecentBlogPosts", builder =>
                    {
                        builder
                            .WithField("displayText");
                    });
                var nodes = result["data"]["recentBlogPosts"];
                if (nodes.AsArray().Count != 2)
                {
                    return true; // next loop
                }
                Assert.Equal(2, nodes.AsArray().Count);
                Assert.Equal("Some sorta blogpost in a Query!", nodes[0]["displayText"].ToString());
                Assert.Equal("Man must explore, and this is exploration at its greatest", nodes[1]["displayText"].ToString());

                return false;

            }, "The Lucene index wasn't updated after the import within 5s and thus the test timed out.");


        }
    }
}
