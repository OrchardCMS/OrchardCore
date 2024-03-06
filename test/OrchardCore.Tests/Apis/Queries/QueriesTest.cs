using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.Queries;
public class QueriesTest
{
    [Fact]
    public async Task ValidateTheQueryResultResponse()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();
        await context.RunRecipeAsync("TestQueryRecipe.json");

        var fistResponse = await context.Client.GetAsync("api/queries/TakeBlogPosts?parameters={\"size\":0}");
        var result1 = await fistResponse.Content.ReadAsStringAsync();

        Assert.NotNull(result1);
        var blogChineseTitle = "中文标题";
        var blogPostContentItemId = await context
            .CreateContentItem("BlogPost", builder =>
            {
                builder.Published = true;
                builder.Latest = true;
                builder.DisplayText = blogChineseTitle;

                builder
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId
                    });
            });
        var response = await context.Client.GetAsync("api/queries/TakeBlogPosts?parameters={\"size\":1}");
        var result2 = await response.Content.ReadAsStringAsync();

        // the result should be include Chinese title
        Assert.Contains(blogChineseTitle, result2);
    }
}
