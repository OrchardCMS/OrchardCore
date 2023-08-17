using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class ContentStepLuceneQueryTests
    {
        [Fact]
        public async Task ShouldUpdateLuceneIndexesOnImport()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemVersionId)] = "newVersion";
                jItem[nameof(ContentItem.DisplayText)] = "new version";
            });

            // Create a second content item in the recipe data so we can confirm the behaviour
            // of the LuceneIndexingContentHandler.
            var data = recipe["steps"][0]["Data"] as JArray;
            var secondContentItem = JObject.FromObject(context.OriginalBlogPost);
            secondContentItem[nameof(ContentItem.ContentItemId)] = "secondcontentitemid";
            secondContentItem[nameof(ContentItem.ContentItemVersionId)] = "secondcontentitemversionid";
            secondContentItem[nameof(ContentItem.DisplayText)] = "second content item display text";
            secondContentItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "new-path";
            data.Add(secondContentItem);

            await context.PostRecipeAsync(recipe);

            // Test
            var result = await context
                .GraphQLClient
                .Content
                .Query("RecentBlogPosts", builder =>
                {
                    builder
                        .WithField("displayText");
                });

            var nodes = result["data"]["recentBlogPosts"];

            Assert.Equal(2, nodes.Count());
            Assert.Equal("new version", nodes[0]["displayText"].ToString());
            Assert.Equal("second content item display text", nodes[1]["displayText"].ToString());
        }
    }
}
