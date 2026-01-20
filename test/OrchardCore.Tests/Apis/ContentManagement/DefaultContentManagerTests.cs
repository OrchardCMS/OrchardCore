using OrchardCore.ContentManagement;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.ContentManagement;

public class DefaultContentManagerTests
{
    [Fact]
    public async Task GetAsync_WithMultipleContentItemIds_ShouldReturnItemsInSameOrderAsInput()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // Create multiple blog posts
        var blogPost1Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "First Blog Post";
        });

        var blogPost2Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "Second Blog Post";
        });

        var blogPost3Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "Third Blog Post";
        });

        // Test with different input orders to ensure ordering is preserved
        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            // Test case 1: Original order
            var inputOrder1 = new[] { blogPost1Id, blogPost2Id, blogPost3Id };
            var result1 = await contentManager.GetAsync(inputOrder1);
            var resultIds1 = result1.Select(item => item.ContentItemId).ToArray();

            Assert.Equal(3, resultIds1.Length);
            Assert.Equal(blogPost1Id, resultIds1[0]);
            Assert.Equal(blogPost2Id, resultIds1[1]);
            Assert.Equal(blogPost3Id, resultIds1[2]);

            // Test case 2: Reverse order
            var inputOrder2 = new[] { blogPost3Id, blogPost2Id, blogPost1Id };
            var result2 = await contentManager.GetAsync(inputOrder2);
            var resultIds2 = result2.Select(item => item.ContentItemId).ToArray();

            Assert.Equal(3, resultIds2.Length);
            Assert.Equal(blogPost3Id, resultIds2[0]);
            Assert.Equal(blogPost2Id, resultIds2[1]);
            Assert.Equal(blogPost1Id, resultIds2[2]);

            // Test case 3: Mixed order
            var inputOrder3 = new[] { blogPost2Id, blogPost1Id, blogPost3Id };
            var result3 = await contentManager.GetAsync(inputOrder3);
            var resultIds3 = result3.Select(item => item.ContentItemId).ToArray();

            Assert.Equal(3, resultIds3.Length);
            Assert.Equal(blogPost2Id, resultIds3[0]);
            Assert.Equal(blogPost1Id, resultIds3[1]);
            Assert.Equal(blogPost3Id, resultIds3[2]);
        });
    }

    [Fact]
    public async Task GetAsync_WithDuplicateIds_ShouldReturnUniqueItemsInCorrectOrder()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // Create blog posts
        var blogPost1Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "First Blog Post";
        });

        var blogPost2Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "Second Blog Post";
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            // Input with duplicates - the method should handle this by removing duplicates
            // but preserving the order of first occurrence
            var inputWithDuplicates = new[] { blogPost2Id, blogPost1Id, blogPost2Id, blogPost1Id };
            var result = await contentManager.GetAsync(inputWithDuplicates);
            var resultIds = result.Select(item => item.ContentItemId).ToArray();

            // Should return unique items in order of first occurrence
            Assert.Equal(2, resultIds.Length);
            Assert.Equal(blogPost2Id, resultIds[0]); // First occurrence in input
            Assert.Equal(blogPost1Id, resultIds[1]); // Second occurrence in input
        });
    }

    [Fact]
    public async Task GetAsync_WithPartiallyExistingIds_ShouldReturnFoundItemsInCorrectOrder()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // Create only some blog posts
        var blogPost1Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "First Blog Post";
        });

        var blogPost3Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "Third Blog Post";
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            // Input includes non-existent ID
            var inputOrder = new[] { blogPost3Id, "non-existent-id", blogPost1Id };
            var result = await contentManager.GetAsync(inputOrder);
            var resultIds = result.Select(item => item.ContentItemId).ToArray();

            // Should return only found items in correct order
            Assert.Equal(2, resultIds.Length);
            Assert.Equal(blogPost3Id, resultIds[0]); // First valid ID in input order
            Assert.Equal(blogPost1Id, resultIds[1]); // Second valid ID in input order
        });
    }

    [Fact]
    public async Task GetAsync_WithEmptyInput_ShouldReturnEmptyResult()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            var result = await contentManager.GetAsync(Array.Empty<string>());

            Assert.Empty(result);
        });
    }

    [Fact]
    public async Task GetAsync_WithNullInput_ShouldReturnEmptyResult()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            var result = await contentManager.GetAsync((IEnumerable<string>)null);

            Assert.Empty(result);
        });
    }

    [Fact]
    public async Task GetAsync_WithVersionOptions_ShouldMaintainOrderRegardlessOfVersionRequested()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // Create and publish blog posts
        var blogPost1Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "First Blog Post";
        });

        var blogPost2Id = await context.CreateContentItem("BlogPost", builder =>
        {
            builder.DisplayText = "Second Blog Post";
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            var inputOrder = new[] { blogPost2Id, blogPost1Id };

            // Test with different version options
            var publishedResult = await contentManager.GetAsync(inputOrder, VersionOptions.Published);
            var publishedIds = publishedResult.Select(item => item.ContentItemId).ToArray();

            var latestResult = await contentManager.GetAsync(inputOrder, VersionOptions.Latest);
            var latestIds = latestResult.Select(item => item.ContentItemId).ToArray();

            // Both should maintain the same order
            Assert.Equal(2, publishedIds.Length);
            Assert.Equal(blogPost2Id, publishedIds[0]);
            Assert.Equal(blogPost1Id, publishedIds[1]);

            Assert.Equal(2, latestIds.Length);
            Assert.Equal(blogPost2Id, latestIds[0]);
            Assert.Equal(blogPost1Id, latestIds[1]);
        });
    }

    [Fact]
    public async Task GetAsync_WithLargeSetOfIds_ShouldMaintainOrderAtScale()
    {
        using var context = new BlogContext();
        await context.InitializeAsync();

        // Create a larger set of blog posts
        var createdIds = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var blogPostId = await context.CreateContentItem("BlogPost", builder =>
            {
                builder.DisplayText = $"Blog Post {i}";
            });
            createdIds.Add(blogPostId);
        }

        await context.UsingTenantScopeAsync(async scope =>
        {
            var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

            // Create a shuffled order different from creation order
            var shuffledOrder = createdIds.ToArray();
            // Reverse the order to ensure it's different from creation order
            Array.Reverse(shuffledOrder);

            var result = await contentManager.GetAsync(shuffledOrder);
            var resultIds = result.Select(item => item.ContentItemId).ToArray();

            Assert.Equal(10, resultIds.Length);

            // Verify the order matches exactly the input order
            for (int i = 0; i < shuffledOrder.Length; i++)
            {
                Assert.Equal(shuffledOrder[i], resultIds[i]);
            }
        });
    }
}
