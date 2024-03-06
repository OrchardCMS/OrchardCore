using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Modules;
using OrchardCore.Tests.Apis.Context;
using Parlot.Fluent;

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

            var result = await context
                .GraphQLClient
                .Content
                .Query("RecentBlogPosts", builder =>
                {
                    builder
                        .WithField("displayText");
                });

            var nodes = result["data"]["recentBlogPosts"];

            Assert.Equal(2, nodes.AsArray().Count());
            Assert.Equal("Some sorta blogpost in a Query!", nodes[0]["displayText"].ToString());
            Assert.Equal("Man must explore, and this is exploration at its greatest", nodes[1]["displayText"].ToString());
        }

        [Fact]
        public async Task ValidateTheGraphqlQueryResultResponse()
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

            var result = await context
                .GraphQLClient
                .Content
                .Query("RecentBlogPosts", builder =>
                {
                    builder
                        .WithField("displayText");
                });

            var nodes = result["data"]["recentBlogPosts"];

            Assert.Equal(2, nodes.AsArray().Count());
            Assert.Equal("Some sorta blogpost in a Query!", nodes[0]["displayText"].ToString());
            Assert.Equal("Man must explore, and this is exploration at its greatest", nodes[1]["displayText"].ToString());
        }

    }
}
