using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using Xunit;
using YesSql;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class BlogPostUpdateDeploymentPlanTests
    {
        [Fact]
        public async Task ShouldUpdateExistingContentItemVersion()
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync();

                // Act
                var recipe = context.MutateContentItem(context.OriginalBlogPost, jItem =>
                {
                    jItem["DisplayText"] = "existingVersion";
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

                        Assert.Single(blogPosts);
                        var originalVersion = blogPosts.First(x => x.ContentItemVersionId == context.OriginalBlogPostVersionId);
                        Assert.Equal("existingVersion", originalVersion.DisplayText);
                    });
                }
            }
        }
    }
}
