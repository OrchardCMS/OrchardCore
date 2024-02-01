using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Tests.Apis.Context;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class BlogPostCreateDeploymentPlanTests
    {
        [Fact]
        public async Task ShouldCreateNewPublishedContentItemVersion()
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

        [Fact]
        public async Task ShouldDiscardDraftThenCreateNewPublishedContentItemVersion()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
            var draftContentItemVersionId = (await content.Content.ReadAsAsync<ContentItem>()).ContentItemVersionId;

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
                {
                    jItem[nameof(ContentItem.ContentItemVersionId)] = "newversion";
                    jItem[nameof(ContentItem.DisplayText)] = "new version";
                });

            await context.PostRecipeAsync(recipe);

            // Test
            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost").ListAsync();

                Assert.Equal(3, blogPosts.Count());
                var originalVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                Assert.False(originalVersion?.Latest);
                Assert.False(originalVersion?.Published);

                var draftVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == draftContentItemVersionId);
                Assert.False(draftVersion?.Latest);
                Assert.False(draftVersion?.Published);

                var newVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == "newversion");
                Assert.Equal("new version", newVersion?.DisplayText);
                Assert.True(newVersion?.Latest);
                Assert.True(newVersion?.Published);
            });
        }

        [Fact]
        public async Task ShouldDiscardDraftThenCreateNewDraftContentItemVersion()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
            var draftContentItemVersionId = (await content.Content.ReadAsAsync<ContentItem>()).ContentItemVersionId;

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemVersionId)] = "newdraftversion";
                jItem[nameof(ContentItem.DisplayText)] = "new draft version";
                jItem[nameof(ContentItem.Published)] = false;
            });

            await context.PostRecipeAsync(recipe);

            // Test
            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost").ListAsync();

                Assert.Equal(3, blogPosts.Count());

                var originalVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                Assert.False(originalVersion?.Latest);
                Assert.True(originalVersion?.Published);

                var draftVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == draftContentItemVersionId);
                Assert.False(draftVersion?.Latest);
                Assert.False(draftVersion?.Published);

                var newDraftVersion = blogPosts.FirstOrDefault(x => x.ContentItemVersionId == "newdraftversion");
                Assert.Equal("new draft version", newDraftVersion?.DisplayText);
                Assert.True(newDraftVersion?.Latest);
                Assert.False(newDraftVersion?.Published);
            });
        }

        [Fact]
        public async Task ShouldCreateNewPublishedContentItem()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            // Act
            var recipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemId)] = "newcontentitemid";
                jItem[nameof(ContentItem.ContentItemVersionId)] = "newversion";
                jItem[nameof(ContentItem.DisplayText)] = "new version";
                jItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "blog/another";
            });

            await context.PostRecipeAsync(recipe);

            // Test
            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPostsCount = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost").CountAsync();

                Assert.Equal(2, blogPostsCount);
            });
        }

        [Fact]
        public async Task ShouldIgnoreDuplicateContentItems()
        {
            using var context = new BlogPostDeploymentContext();

            // Setup
            await context.InitializeAsync();

            // Create a recipe with two content items and the same version id.
            var firstRecipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemId)] = "newcontentitemid";
                jItem[nameof(ContentItem.ContentItemVersionId)] = "dupversion";
                jItem[nameof(ContentItem.DisplayText)] = "duplicate version";
                jItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "blog/another";
            });

            var secondRecipe = BlogPostDeploymentContext.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
            {
                jItem[nameof(ContentItem.ContentItemId)] = "newcontentitemid";
                jItem[nameof(ContentItem.ContentItemVersionId)] = "dupversion";
                jItem[nameof(ContentItem.DisplayText)] = "duplicate version";
                jItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "blog/another";
            });

            var firstRecipeData = firstRecipe.SelectToken("steps[0].Data") as JArray;

            var secondContentItem = secondRecipe.SelectToken("steps[0].Data[0]");

            firstRecipeData.Add(secondContentItem);

            await context.PostRecipeAsync(firstRecipe);

            // Test
            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPostsCount = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost" && x.ContentItemVersionId == "dupversion").CountAsync();

                Assert.Equal(1, blogPostsCount);
            });
        }
    }
}
