using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Utilities;

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
            var data = recipe["steps"][0]["Data"].AsArray();
            var secondContentItem = JObject.FromObject(context.OriginalBlogPost);
            secondContentItem[nameof(ContentItem.ContentItemId)] = "secondcontentitemid";
            secondContentItem[nameof(ContentItem.ContentItemVersionId)] = "secondcontentitemversionid";
            secondContentItem[nameof(ContentItem.DisplayText)] = "second content item display text";
            secondContentItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "new-path";
            data.Add(secondContentItem);

            await context.PostRecipeAsync(recipe);

            // Search indexes are no longer updated in a deferred task at the end of a shell scope
            // but in a background job after the http request, so they are not already up to date.

            await TimeoutTaskRunner.RunAsync(TimeSpan.FromSeconds(5), async () =>
            {
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

                return nodes is not null
                    && nodes.AsArray().Count == 2
                    && "new version" == nodes[0]["displayText"].ToString()
                    && "second content item display text" == nodes[1]["displayText"].ToString();
            }, "The Lucene index wasn't updated after the import within 5s and thus the test timed out.");


        }
    }
}
