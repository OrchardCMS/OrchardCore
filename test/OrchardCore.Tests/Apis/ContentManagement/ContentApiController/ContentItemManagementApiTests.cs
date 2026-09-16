using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Tests.Apis.ContentManagement.ContentApiController;

public class ContentItemManagementApiTests
{
    [Fact]
    public async Task GetDraft_PublishedItem_DoesNotCreateDraft()
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        var id = context.BlogPost.ContentItemId;
        var response = await context.Client.GetAsync($"api/content/{id}?version=draft", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await context.UsingTenantScopeAsync(async scope =>
        {
            var manager = scope.ServiceProvider.GetRequiredService<IContentManager>();
            Assert.Null(await manager.GetAsync(id, VersionOptions.Draft));
            Assert.NotNull(await manager.GetAsync(id, VersionOptions.Published));
        });
    }

    [Fact]
    public async Task Update_RouteBodyIdMismatch_ReturnsValidationProblem()
    {
        using var context = new BlogPostApiControllerContext();
        await context.InitializeAsync();
        var response = await context.Client.PutAsJsonAsync($"api/content/{context.BlogPost.ContentItemId}", new ContentItem
        {
            ContentItemId = "different-id",
            ContentType = "BlogPost",
        });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ContentService_ListsGetsAndDescribesSchema()
    {
        using var context = new SiteContext();

        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<ContentApiService>();
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Test")], nameof(StubIdentity)));

            var list = await service.ListAsync(user, "BlogPost", "published", 0, 10);
            Assert.True(list.TotalCount >= 1);

            var contentItemId = Assert.Single(list.Items.Take(1)).ContentItemId;
            var contentItem = await service.GetAsync(contentItemId, "published");
            var schema = await service.GetSchemaAsync("BlogPost");

            Assert.IsType<ContentItem>(contentItem);
            Assert.Equal(contentItemId, contentItem.ContentItemId);
            Assert.Equal("BlogPost", schema.ContentType);
            Assert.Equal("BlogPost", schema.Schema["properties"]?[nameof(ContentItem.ContentType)]?["const"]?.ToString());
        });
    }

    [Fact]
    public async Task CanViewContentItemAsync_PublishedItem_RequiresViewPermission()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var contentItem = new ContentItem { Published = true };
        Permission requestedPermission = null;
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                user,
                contentItem,
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .Callback<ClaimsPrincipal, object, IEnumerable<IAuthorizationRequirement>>((_, _, requirements) =>
                requestedPermission = Assert.Single(requirements.OfType<PermissionRequirement>()).Permission)
            .ReturnsAsync(AuthorizationResult.Failed());

        var authorized = await ContentApiService.CanViewContentItemAsync(authorizationService.Object, user, contentItem);

        Assert.False(authorized);
        Assert.Equal(CommonPermissions.ViewContent, requestedPermission);
    }

    [Fact]
    public async Task PrepareOwnershipAsync_UnauthorizedOwnerChange_IsRejected()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var model = new ContentItem
        {
            Owner = "another-user-id",
            Author = "spoofed-author",
        };
        var contentItem = new ContentItem
        {
            Owner = "owner-id",
            Author = "actual-author",
        };
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                user,
                contentItem,
                It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                    requirements.OfType<PermissionRequirement>().Any(requirement =>
                        requirement.Permission == CommonPermissions.EditContentOwner))))
            .ReturnsAsync(AuthorizationResult.Failed());

        var authorized = await ContentApiService.PrepareOwnershipAsync(
            authorizationService.Object,
            user,
            model,
            contentItem);

        Assert.False(authorized);
        Assert.Equal("actual-author", model.Author);
    }

    [Fact]
    public async Task ContentService_StableIdAndStateTransitions_AreRetrySafe()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            const string contentItemId = "idempotent-content-item";
            var service = scope.ServiceProvider.GetRequiredService<ContentApiService>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, "Test")],
                nameof(StubIdentity)));

            static ContentItem CreateModel() => new()
            {
                ContentItemId = contentItemId,
                ContentType = "BlogPost",
                DisplayText = "Retry-safe content",
            };

            var firstSave = await service.SaveAsync(user, CreateModel(), publish: false, allowCreate: true, allowUpdate: true);
            var secondSave = await service.SaveAsync(user, CreateModel(), publish: false, allowCreate: true, allowUpdate: true);
            var firstPublish = await service.PublishAsync(user, contentItemId);
            var secondPublish = await service.PublishAsync(user, contentItemId);
            var firstUnpublish = await service.UnpublishAsync(user, contentItemId);
            var secondUnpublish = await service.UnpublishAsync(user, contentItemId);

            Assert.Equal(contentItemId, firstSave.ContentItemId);
            Assert.Equal(contentItemId, secondSave.ContentItemId);
            Assert.Equal(contentItemId, firstPublish.ContentItemId);
            Assert.Equal(contentItemId, secondPublish.ContentItemId);
            Assert.Equal(contentItemId, firstUnpublish.ContentItemId);
            Assert.Equal(contentItemId, secondUnpublish.ContentItemId);
        });
    }
}
