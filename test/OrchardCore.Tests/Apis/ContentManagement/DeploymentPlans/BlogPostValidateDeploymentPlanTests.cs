using System.Net.Http;
using System.Threading.Tasks;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;
using Xunit;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class BlogPostFailValidationDeploymentPlanTests
    {
        [Fact]
        public async Task ShouldFailWhenAutoroutePathIsNotUnique()
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync();

                // Act
                var recipe = context.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
                {
                    jItem[nameof(ContentItem.ContentItemId)] = "newContentItemid";
                    jItem[nameof(ContentItem.ContentItemVersionId)] = "newVersion";
                    jItem[nameof(ContentItem.DisplayText)] = "new version";
                    // To be explicit in this test set path to a known value.
                    jItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "blog/post-1";
                });

                var response = await context.PostRecipeAsync(recipe, false);

                // Test
                Assert.Throws<HttpRequestException>(() => response.EnsureSuccessStatusCode());
            }
        }

    }
}
