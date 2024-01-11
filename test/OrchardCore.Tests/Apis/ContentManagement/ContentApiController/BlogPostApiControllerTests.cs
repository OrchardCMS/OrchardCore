using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Lists.Models;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Tests.Apis.Context;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.ContentManagement.ContentApiController
{
    public class BlogPostApiControllerTests
    {
        [Fact]
        public async Task ShouldCreateDraftOfExistingContentItem()
        {
            using var context = new BlogPostApiControllerContext();

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

        [Fact]
        public async Task ShouldCreateAndPublishExistingContentItem()
        {
            using var context = new BlogPostApiControllerContext();

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

        [Fact]
        public async Task ShouldOnlyCreateTwoContentItemRecordsForExistingContentItem()
        {
            using var context = new BlogPostApiControllerContext();

            // Setup
            await context.InitializeAsync();

            context.BlogPost.Latest = false;
            context.BlogPost.Published = false; // Deliberately set these incorrectly.

            // Act
            await context.Client.PostAsJsonAsync("api/content", context.BlogPost);

            // Test
            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost").ListAsync();

                Assert.Equal(2, blogPosts.Count());
            });
        }

        [Fact]
        public async Task ShouldCreateDraftOfNewContentItem()
        {
            using var context = new BlogPostApiControllerContext();

            // Setup
            var displayText = "some other blog post";
            await context.InitializeAsync();

            var contentItem = new ContentItem
            {
                ContentType = "BlogPost",
                DisplayText = displayText,
                Latest = true,
                Published = true, // Deliberately set these values incorrectly.
            };

            contentItem
                .Weld(new AutoroutePart
                {
                    Path = "Path2",
                });

            contentItem
                .Weld(new ContainedPart
                {
                    ListContentItemId = context.BlogContentItemId
                });

            var blogFields = new ContentPart();
            blogFields
                .Weld("Categories", new TaxonomyField
                {
                    TaxonomyContentItemId = context.CategoriesTaxonomyContentItemId,
                });

            blogFields
                .Weld("Tags", new TaxonomyField
                {
                    TaxonomyContentItemId = context.TagsTaxonomyContentItemId,
                });

            contentItem
                .Weld("BlogPost", blogFields);

            // Act
            var content = await context.Client.PostAsJsonAsync("api/content?draft=true", contentItem);
            var draftContentItem = await content.Content.ReadAsAsync<ContentItem>();

            // Test
            Assert.True(draftContentItem.Latest);
            Assert.False(draftContentItem.Published);
            Assert.Equal(displayText, draftContentItem.DisplayText);
            Assert.NotNull(draftContentItem.As<AutoroutePart>());
        }

        [Fact]
        public async Task ShouldCreateAndPublishNewContentItem()
        {
            using var context = new BlogPostApiControllerContext();

            // Setup
            var displayText = "some other blog post";
            var path = "path2";
            await context.InitializeAsync();

            var contentItem = new ContentItem
            {
                ContentType = "BlogPost",
                DisplayText = displayText,
                Latest = false,
                Published = false, // Deliberately set these values incorrectly.
            };

            contentItem
                .Weld(new AutoroutePart
                {
                    Path = path,
                });

            contentItem
                .Weld(new ContainedPart
                {
                    ListContentItemId = context.BlogContentItemId,
                });

            var blogFields = new ContentPart();
            blogFields
                .Weld("Categories", new TaxonomyField
                {
                    TaxonomyContentItemId = context.CategoriesTaxonomyContentItemId,
                });

            blogFields
                .Weld("Tags", new TaxonomyField
                {
                    TaxonomyContentItemId = context.TagsTaxonomyContentItemId,
                });

            contentItem
                .Weld("BlogPost", blogFields);

            // Act
            var content = await context.Client.PostAsJsonAsync("api/content", contentItem);
            var publishedContentItem = await content.Content.ReadAsAsync<ContentItem>();

            // Test
            Assert.True(publishedContentItem.Latest);
            Assert.True(publishedContentItem.Published);
            Assert.Equal(displayText, publishedContentItem.DisplayText);
            Assert.Equal(path, publishedContentItem.As<AutoroutePart>()?.Path);
        }

        [Fact]
        public async Task ShouldFailValidationWhenAutoroutePathIsNotUnique()
        {
            using var context = new BlogPostApiControllerContext();

            // Setup
            await context.InitializeAsync();

            var contentItem = new ContentItem
            {
                ContentType = "BlogPost",
                DisplayText = "some other blog post",
                Latest = false,
                Published = false, // Deliberately set these values incorrectly.
            };

            contentItem
                .Weld(new AutoroutePart
                {
                    Path = "blog/post-1", // Deliberately set to an existing path.
                });

            contentItem
                .Weld(new ContainedPart
                {
                    ListContentItemId = context.BlogContentItemId,
                });

            var blogFields = new ContentPart();
            blogFields
                .Weld("Categories", new TaxonomyField
                {
                    TaxonomyContentItemId = context.CategoriesTaxonomyContentItemId,
                });

            blogFields
                .Weld("Tags", new TaxonomyField
                {
                    TaxonomyContentItemId = context.TagsTaxonomyContentItemId,
                });

            contentItem
                .Weld("BlogPost", blogFields);

            // Act
            var result = await context.Client.PostAsJsonAsync("api/content", contentItem);
            var problemDetails = await result.Content.ReadAsAsync<ProblemDetails>();

            // Test
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
            Assert.Contains("Your permalink is already in use.", problemDetails.Detail);

            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var blogPosts = await session.Query<ContentItem, ContentItemIndex>(x =>
                    x.ContentType == "BlogPost").ListAsync();

                Assert.Single(blogPosts);
            });
        }

        [Fact]
        public async Task ShouldGenerateUniqueAutoroutePath()
        {
            using var context = new BlogPostApiControllerContext();

            // Setup
            await context.InitializeAsync();

            var contentItem = new ContentItem
            {
                ContentType = "BlogPost",
                DisplayText = "some other blog post",
                Latest = false,
                Published = false, // Deliberately set these values incorrectly.
            };

            contentItem
                .Weld(new ContainedPart
                {
                    ListContentItemId = context.BlogContentItemId,
                });

            var blogFields = new ContentPart();
            blogFields
                .Weld("Categories", new TaxonomyField
                {
                    TaxonomyContentItemId = context.CategoriesTaxonomyContentItemId,
                });

            blogFields
                .Weld("Tags", new TaxonomyField
                {
                    TaxonomyContentItemId = context.TagsTaxonomyContentItemId,
                });

            contentItem
                .Weld("BlogPost", blogFields);

            // Act
            var content = await context.Client.PostAsJsonAsync("api/content", contentItem);
            var publishedContentItem = await content.Content.ReadAsAsync<ContentItem>();

            // Test
            var blogPostContentItemIds = new List<string>
            {
                context.BlogPost.ContentItemId,
                publishedContentItem.ContentItemId,
            };

            await context.UsingTenantScopeAsync(async scope =>
            {
                var session = scope.ServiceProvider.GetRequiredService<ISession>();
                var newAutoroutePartIndex = await session
                    .QueryIndex<AutoroutePartIndex>(o => o.Published && o.ContentItemId == publishedContentItem.ContentItemId)
                    .FirstOrDefaultAsync();

                // The Autoroute part was not welded on, so ContentManager.NewAsync should add it
                // with an empty path and then generate a unique path from the liquid pattern.
                Assert.Equal("blog/some-other-blog-post", publishedContentItem.As<AutoroutePart>().Path);
            });
        }
    }
}
