using System.Text.Json.Nodes;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Indexing;
using OrchardCore.Queries;
using OrchardCore.Queries.Sql;
using OrchardCore.Search.Lucene;
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

            Assert.Equal(2, nodes.AsArray().Count);
            Assert.Equal("new version", nodes[0]["displayText"].ToString());
            Assert.Equal("second content item display text", nodes[1]["displayText"].ToString());
        }



        [Fact]
        public async Task ShouldLuceneIndexOnlyIndexesTheLatestVersionOnImport()
        {
            using var context = new BlogPostDeploymentContext();
            await context.InitializeAsync();
            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemVersionId)] = "newVersion";
                jItem[nameof(ContentItem.DisplayText)] = "new version";
            });
            const string secondcontentitemversionid = "secondcontentitemversionid";
            var data = recipe["steps"][0]["Data"].AsArray();
            var secondContentItem = JObject.FromObject(context.OriginalBlogPost);
            secondContentItem[nameof(ContentItem.ContentItemVersionId)] = secondcontentitemversionid;
            secondContentItem[nameof(ContentItem.DisplayText)] = "second content item version display text";
            secondContentItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "new-path";
            data.Add(secondContentItem);

            Assert.Equal(2, data.Count);
            Assert.Single(data.Select(x => x.SelectNode(nameof(ContentItem.ContentItemId)).ToString()).Distinct());

            // validate different contentItem versionId
            var versionIds = data.Select(x => x.SelectNode(nameof(ContentItem.ContentItemVersionId)).ToString()).Distinct();
            Assert.Equal(2, versionIds.Count());

            await context.PostRecipeAsync(recipe);

            // Search indexes are no longer updated in a deferred task at the end of a shell scope
            // but in a background job after the http request, so they are not already up to date.

            await context.UsingTenantScopeAsync(async scope =>
            {
                var queryManager = scope.ServiceProvider.GetRequiredService<IQueryManager>();


                var luceneQuery = new LuceneQuery
                {
                    Index = "Search",
                    ReturnContentItems = false,
                    Template = @"
                            {
                                ""query"": {
                                    ""term"": { ""Content.ContentItem.ContentType"": ""BlogPost"" }
                                },
                                ""filter"": [
                                    {
                                        ""term"": { ""Content.ContentItem.Published"":""true"" }
                                    },
                                    {
                                        ""term"": { ""Content.ContentItem.ContentItemId"":""{{parameters.contentItemId}}"" }
                                    }
                                ]
                            }",
                };
                var queryParameters = new Dictionary<string, object>
                {
                    ["contentItemId"] = context.OriginalBlogPost.ContentItemId
                };

                var loopCount = 4;
                var luceneQuerySource = scope.ServiceProvider.GetRequiredService<LuceneQuerySource>();
                await TimeoutTaskRunner.RunAsync(new TimeoutTaskOption
                {
                    Timeout = TimeSpan.FromSeconds(5),
                    Interval = 500,
                    TimeoutMessage = "The Lucene index wasn't updated after the import within 5s and thus the test timed out.",
                    CanNextLoop = async () =>
                                       {
                                           loopCount++;
                                           // Ensure adequate waiting time.
                                           if (loopCount < 4)
                                           {
                                               return true; 
                                           }

                                           var result = await luceneQuerySource.ExecuteQueryAsync(luceneQuery, queryParameters);

                                           if (result is not null)
                                           {
                                               if (result.Items.Count() > 1)
                                               {
                                                   Assert.Fail("The Lucene index should only index latest version on BlogPost type.");
                                                   return false;
                                               }
                                               if (result.Items.Count() == 1
                                                    && JObject.FromObject(result).SelectNode("$.items[0].ContentItemVersionId").ToString() == secondcontentitemversionid)
                                               {
                                                   return false;
                                               }
                                           }
                                           return true; // continue;
                                       }
                });
            });
        }
    }
}
