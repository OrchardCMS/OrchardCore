using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Tests.Apis.Context;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class BlogPostContentStepIdempotentTests
    {
        [Fact]
        public async Task ShouldProduceSameOutcomeForNewContentOnMultipleExecutions()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemVersionId)] = "newversion";
                jItem[nameof(ContentItem.DisplayText)] = "new version";
            });

            for (var i = 0; i < 2; i++)
            {
                await context.PostRecipeAsync(recipe);

                // Test
                await context.UsingTenantScopeAsync(async scope =>
                {
                    var session = scope.ServiceProvider.GetRequiredService<ISession>();
                    var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == "BlogPost").ListAsync();

                    Assert.Equal(2, blogPosts.Count());

                    var originalVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                    Assert.False(originalVersion?.Latest);
                    Assert.False(originalVersion?.Published);

                    var newVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == "newversion");
                    Assert.Equal("new version", newVersion?.DisplayText);
                    Assert.True(newVersion?.Latest);
                    Assert.True(newVersion?.Published);
                });
            }
        }

        [Fact]
        public async Task ShouldProduceSameOutcomeForExistingContentItemVersionOnMultipleExecutions()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.DisplayText)] = "existing version mutated";
            });

            for (var i = 0; i < 2; i++)
            {
                await context.PostRecipeAsync(recipe);

                // Test
                await context.UsingTenantScopeAsync(async scope =>
                {
                    var session = scope.ServiceProvider.GetRequiredService<ISession>();
                    var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == "BlogPost").ListAsync();

                    Assert.Single(blogPosts);
                    var mutatedVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                    Assert.Equal("existing version mutated", mutatedVersion?.DisplayText);
                });
            }
        }

    }
}
