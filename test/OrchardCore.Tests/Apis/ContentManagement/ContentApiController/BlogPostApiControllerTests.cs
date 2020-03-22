using System.Threading.Tasks;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Environment.Shell;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using YesSql;
using OrchardCore.ContentManagement.Records;
using System.Linq;

namespace OrchardCore.Tests.Apis.ContentManagement.ContentApiController
{
    public class BlogPostApiControllerTests
    {
        [Fact]
        public async Task ShouldCreateDraftOfExistingContentItem()
        {
            using (var context = new BlogPostApiControllerContext())
            {
                // Setup
                await context.InitializeAsync();

                context.BlogPost.Latest = false;
                context.BlogPost.Published = true; // Deliberately set these incorrectly.

                // Act
                var content = await context.Client.PostAsJsonAsync("api/content?draft=true", context.BlogPost);
                var draftContentItem = await content.Content.ReadAsAsync<ContentItem>();

                // Test
                Assert.True(draftContentItem.Latest);
                Assert.False(draftContentItem.Published);
            }
        }

        [Fact]
        public async Task ShouldCreateAndPublishExistingContentItem()
        {
            using (var context = new BlogPostApiControllerContext())
            {
                // Setup
                await context.InitializeAsync();

                context.BlogPost.Latest = false;
                context.BlogPost.Published = false; // Deliberately set these incorrectly.

                // Act
                var content = await context.Client.PostAsJsonAsync("api/content", context.BlogPost);
                var draftContentItem = await content.Content.ReadAsAsync<ContentItem>();

                // Test
                Assert.True(draftContentItem.Latest);
                Assert.True(draftContentItem.Published);
            }
        }

        [Fact]
        public async Task ShouldOnlyCreateTwoContentItemRecordsForExistingContentItem()
        {
            using (var context = new BlogPostApiControllerContext())
            {
                // Setup
                await context.InitializeAsync();

                context.BlogPost.Latest = false;
                context.BlogPost.Published = false; // Deliberately set these incorrectly.

                // Act
                await context.Client.PostAsJsonAsync("api/content", context.BlogPost);

                // Test
                using (var shellScope = await BlogPostApiControllerContext.ShellHost.GetScopeAsync(context.TenantName))
                {
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

        [Fact]
        public async Task ShouldCreateDraftOfNewContentItem()
        {
            using (var context = new BlogPostApiControllerContext())
            {
                // Setup
                var displayText = "some other blog post";
                await context.InitializeAsync();

                var contentItem = new ContentItem
                {
                    ContentType = "BlogPost",
                    DisplayText = displayText,
                    Latest = true,
                    Published = true // Deliberately set these values incorrectly
                };

                contentItem
                    .Weld(new AutoroutePart
                    {
                        Path = "Path2"
                    });

                contentItem
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId
                    });

                // Act
                var content = await context.Client.PostAsJsonAsync("api/content?draft=true", contentItem);
                var draftContentItem = await content.Content.ReadAsAsync<ContentItem>();

                // Test
                Assert.True(draftContentItem.Latest);
                Assert.False(draftContentItem.Published);
                Assert.Equal(displayText, draftContentItem.DisplayText);
                Assert.NotNull(draftContentItem.As<AutoroutePart>());
            }
        }

        [Fact]
        public async Task ShouldCreateAndPublishNewContentItem()
        {
            using (var context = new BlogPostApiControllerContext())
            {
                // Setup
                var displayText = "some other blog post";
                var path = "path2";
                await context.InitializeAsync();

                var contentItem = new ContentItem
                {
                    ContentType = "BlogPost",
                    DisplayText = displayText,
                    Latest = false,
                    Published = false // Deliberately set these values incorrectly
                };

                contentItem
                    .Weld(new AutoroutePart
                    {
                        Path = path
                    });

                contentItem
                    .Weld(new ContainedPart
                    {
                        ListContentItemId = context.BlogContentItemId
                    });

                // Act
                var content = await context.Client.PostAsJsonAsync("api/content", contentItem);
                var draftContentItem = await content.Content.ReadAsAsync<ContentItem>();

                // Test
                Assert.True(draftContentItem.Latest);
                Assert.True(draftContentItem.Published);
                Assert.Equal(displayText, draftContentItem.DisplayText);
                Assert.Equal(path, draftContentItem.As<AutoroutePart>()?.Path);
            }
        }
    }
}
