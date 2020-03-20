using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using Xunit;
using YesSql;

namespace OrchardCore.Tests.Apis.ContentManagement
{
    public class BlogPostCreateDeploymentPlanTests
    {
        [Fact]
        public async Task ShouldCreateNewPublishedContentItemVersion()
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync();

                // Act
                var recipe = context.MutateContentItem(context.OriginalBlogPost, jItem =>
                {
                    jItem["ContentItemVersionId"] = "newVersion";
                    jItem["DisplayText"] = "newVersion";
                });

                await context.PostRecipeAsync(recipe);

                // Test
                using (var shellScope = await BlogPostDeploymentContext.ShellHost.GetScopeAsync(context.TenantName))
                {
                    await shellScope.UsingAsync(async scope =>
                    {
                        var session = scope.ServiceProvider.GetRequiredService<ISession>();
                        var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                            x.ContentType == "BlogPost").ListAsync();

                        Assert.Equal(2, blogPosts.Count());
                        var originalVersion = blogPosts.First(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                        Assert.False(originalVersion.Latest);
                        Assert.False(originalVersion.Published);
                        var newVersion = blogPosts.First(x => x.ContentItemVersionId == "newVersion");
                        Assert.Equal("newVersion", newVersion.DisplayText);
                        Assert.True(newVersion.Latest);
                        Assert.True(newVersion.Published);
                    });
                }
            }
        }

        [Fact]
        public async Task ShouldDiscardDraftThenCreateNewPublishedContentItemVersion()
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync();

                var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
                var draftContentItemVersionId = (await content.Content.ReadAsAsync<ContentItem>()).ContentItemVersionId;
 
                // Act
                var recipe = context.MutateContentItem(context.OriginalBlogPost, jItem =>
                {
                    jItem["ContentItemVersionId"] = "newVersion";
                    jItem["DisplayText"] = "newVersion";
                });

                await context.PostRecipeAsync(recipe);

                // Test
                using (var shellScope = await BlogPostDeploymentContext.ShellHost.GetScopeAsync(context.TenantName))
                {
                    await shellScope.UsingAsync(async scope =>
                    {
                        var session = scope.ServiceProvider.GetRequiredService<ISession>();
                        var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                            x.ContentType == "BlogPost").ListAsync();

                        Assert.Equal(3, blogPosts.Count());
                        var originalVersion = blogPosts.First(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                        Assert.False(originalVersion.Latest);
                        Assert.False(originalVersion.Published);
                        var draftVersion = blogPosts.First(x => x.ContentItemVersionId == draftContentItemVersionId);
                        Assert.False(draftVersion.Latest);
                        Assert.False(draftVersion.Published);

                        var newVersion = blogPosts.First(x => x.ContentItemVersionId == "newVersion");
                        Assert.Equal("newVersion", newVersion.DisplayText);
                        Assert.True(newVersion.Latest);
                        Assert.True(newVersion.Published);
                    });
                }
            }
        }
    }
}
