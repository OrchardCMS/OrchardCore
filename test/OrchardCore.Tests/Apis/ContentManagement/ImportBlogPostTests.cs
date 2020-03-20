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
    public class ImportBlogPostTests
    {
        [Fact]
        public async Task ShouldImportUpdatedContentItemVersion()
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync();


                // Act
                var recipe = context.MutateContentItem(context.OriginalBlogPost, jItem =>
                {
                    jItem["ContentItemVersionId"] = "updated";
                    jItem["DisplayText"] = "updated";
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
                        var original = blogPosts.First(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                        Assert.False(original.Latest);
                        Assert.False(original.Published);
                        var updated = blogPosts.First(x => x.ContentItemVersionId == "updated");
                        Assert.Equal("updated", updated.DisplayText);
                    });
                }
            }
        }
    }
}
