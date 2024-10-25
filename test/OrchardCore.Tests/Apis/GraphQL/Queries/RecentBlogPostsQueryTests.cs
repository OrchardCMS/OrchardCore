using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.GraphQL;

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

        var jsonArray = result["data"]?["recentBlogPosts"]?.AsArray();

        Assert.NotNull(jsonArray);
        Assert.Equal(2, jsonArray.Count);

        // The RecentBlogPosts query sorts the content items by CreatedUtc. If the
        // test is executing too fast, both blog entries may have the same CreatedUtc
        // value and ordering becomes random. Because of this, we do not assert the order
        // of the result.
        var displayTexts = jsonArray.Select(node => node["displayText"]?.ToString());

        Assert.Contains("Some sorta blogpost in a Query!", displayTexts);
        Assert.Contains("Man must explore, and this is exploration at its greatest", displayTexts);
    }
}
