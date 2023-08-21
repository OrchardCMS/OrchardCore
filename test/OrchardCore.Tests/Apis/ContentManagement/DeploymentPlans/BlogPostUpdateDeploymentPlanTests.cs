using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Tests.Apis.Context;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class BlogPostUpdateDeploymentPlanTests
    {
        [Fact]
        public async Task ShouldUpdateExistingContentItemVersion()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.DisplayText)] = "existing version mutated";
            });

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

        [Fact]
        public async Task ShouldDiscardDraftThenUpdateExistingContentItemVersion()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
            var draftContentItemVersionId = (await content.Content.ReadAsAsync<ContentItem>()).ContentItemVersionId;

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.DisplayText)] = "existing version mutated";
            });

            await context.PostRecipeAsync(recipe);

            // Test
            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost").ListAsync();

                Assert.Equal(2, blogPosts.Count());

                var mutatedVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                Assert.True(mutatedVersion?.Latest);
                Assert.True(mutatedVersion?.Published);
                Assert.Equal("existing version mutated", mutatedVersion?.DisplayText);

                var draftVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == draftContentItemVersionId);
                Assert.False(draftVersion.Latest);
            });
        }

        [Fact]
        public async Task ShouldUpdateDraftThenPublishExistingContentItemVersion()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
            var draftContentItem = (await content.Content.ReadAsAsync<ContentItem>());

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(draftContentItem, jItem =>
            {
                jItem[nameof(ContentItem.DisplayText)] = "draft version mutated";
                jItem[nameof(ContentItem.Published)] = true;
                jItem[nameof(ContentItem.Latest)] = true;
            });

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

                var draftVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == draftContentItem.ContentItemVersionId);
                Assert.True(draftVersion?.Latest);
                Assert.True(draftVersion?.Published);
                Assert.Equal("draft version mutated", draftVersion?.DisplayText);
            });
        }
    }
}
