using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Apis.Context.Attributes;
using Xunit;
using YesSql;

namespace OrchardCore.Tests.Apis.ContentManagement.DeploymentPlans
{
    public class BlogPostCreateDeploymentPlanTests
    {
        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldCreateNewPublishedContentItemVersion(string databaseProvider, string connectionString)
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync(databaseProvider, connectionString);

                // Act
                var recipe = context.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
                {
                    jItem[nameof(ContentItem.ContentItemVersionId)] = "newversion";
                    jItem[nameof(ContentItem.DisplayText)] = "new version";
                });

                await context.PostRecipeAsync(recipe);

                // Test
                var shellScope = await BlogPostDeploymentContext.ShellHost.GetScopeAsync(context.TenantName);
                await shellScope.UsingAsync(async scope =>
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

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldDiscardDraftThenCreateNewPublishedContentItemVersion(string databaseProvider, string connectionString)
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync(databaseProvider, connectionString);

                var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
                var draftContentItemVersionId = (await content.Content.ReadAsAsync<ContentItem>()).ContentItemVersionId;

                // Act
                var recipe = context.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
                    {
                        jItem[nameof(ContentItem.ContentItemVersionId)] = "newversion";
                        jItem[nameof(ContentItem.DisplayText)] = "new version";
                    });

                await context.PostRecipeAsync(recipe);

                // Test
                var shellScope = await BlogPostDeploymentContext.ShellHost.GetScopeAsync(context.TenantName);
                await shellScope.UsingAsync(async scope =>
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
                    Assert.Equal("new version", newVersion.DisplayText);
                    Assert.True(newVersion?.Latest);
                    Assert.True(newVersion?.Published);
                });
            }
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldDiscardDraftThenCreateNewDraftContentItemVersion(string databaseProvider, string connectionString)
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync(databaseProvider, connectionString);

                var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.OriginalBlogPost);
                var draftContentItemVersionId = (await content.Content.ReadAsAsync<ContentItem>()).ContentItemVersionId;

                // Act
                var recipe = context.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
                {
                    jItem[nameof(ContentItem.ContentItemVersionId)] = "newdraftversion";
                    jItem[nameof(ContentItem.DisplayText)] = "new draft version";
                    jItem[nameof(ContentItem.Published)] = false;
                });

                await context.PostRecipeAsync(recipe);

                // Test
                var shellScope = await BlogPostDeploymentContext.ShellHost.GetScopeAsync(context.TenantName);
                await shellScope.UsingAsync(async scope =>
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
        }

        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldCreateNewPublishedContentItem(string databaseProvider, string connectionString)
        {
            using (var context = new BlogPostDeploymentContext())
            {
                // Setup
                await context.InitializeAsync(databaseProvider, connectionString);

                // Act
                var recipe = context.GetContentStepRecipe(context.OriginalBlogPost, jItem =>
                {
                    jItem[nameof(ContentItem.ContentItemId)] = "newcontentitemid";
                    jItem[nameof(ContentItem.ContentItemVersionId)] = "newversion";
                    jItem[nameof(ContentItem.DisplayText)] = "new version";
                    jItem[nameof(AutoroutePart)][nameof(AutoroutePart.Path)] = "blog/another";
                });

                await context.PostRecipeAsync(recipe);

                // Test
                var shellScope = await BlogPostDeploymentContext.ShellHost.GetScopeAsync(context.TenantName);
                await shellScope.UsingAsync(async scope =>
                {
                    var session = scope.ServiceProvider.GetRequiredService<ISession>();
                    var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                        x.ContentType == "BlogPost").ListAsync();

                    Assert.Equal(2, blogPosts.Count());
                });
            }
        }
    }
}
