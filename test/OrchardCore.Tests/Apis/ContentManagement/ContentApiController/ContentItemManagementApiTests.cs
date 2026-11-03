using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.ContentManagement;
using OrchardCore.Contents;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.RemoteManagement;
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
    public async Task HeadlessRoutes_ContentApiPermissionOnly_DoNotRequireRemoteManagement()
    {
        var permissions = new PermissionsContext();
        using var context = new BlogPostApiControllerContext().WithPermissionsContext(permissions);
        await context.InitializeAsync();
        var id = context.BlogPost.ContentItemId;
        permissions.UsePermissionsContext = true;

        // The test client has no token, like a public headless client of the Anonymous role.
        permissions.AuthorizedPermissions = [CommonPermissions.AccessContentApi, CommonPermissions.ViewContent];
        Assert.Equal(HttpStatusCode.OK, (await context.Client.GetAsync($"api/content/{id}", TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await context.Client.GetAsync("api/content", TestContext.Current.CancellationToken)).StatusCode);

        permissions.AuthorizedPermissions = [CommonPermissions.ViewContent];
        Assert.Equal(HttpStatusCode.Unauthorized, (await context.Client.GetAsync($"api/content/{id}", TestContext.Current.CancellationToken)).StatusCode);

        permissions.AuthorizedPermissions = [CommonPermissions.AccessContentApi, CommonPermissions.DeleteContent];
        Assert.Equal(HttpStatusCode.OK, (await context.Client.DeleteAsync($"api/content/{id}", TestContext.Current.CancellationToken)).StatusCode);
    }

    [Fact]
    public async Task List_TotalCount_OnlyCountsItemsTheCallerCanView()
    {
        var permissions = new PermissionsContext();
        using var context = new BlogPostApiControllerContext { PermissionsContext = permissions };
        await context.InitializeAsync();
        permissions.UsePermissionsContext = true;

        permissions.AuthorizedPermissions = [RemoteManagementPermissions.AccessRemoteManagement, CommonPermissions.AccessContentApi, CommonPermissions.ListContent, CommonPermissions.ViewContent];
        var all = await (await context.Client.GetAsync("api/content?contentType=BlogPost&take=1", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItemsResponse>();
        Assert.True(all.TotalCount >= 1);
        Assert.Single(all.Items);

        permissions.AuthorizedPermissions = [RemoteManagementPermissions.AccessRemoteManagement, CommonPermissions.AccessContentApi, CommonPermissions.ListContent];
        var none = await (await context.Client.GetAsync("api/content?contentType=BlogPost&take=1", TestContext.Current.CancellationToken)).Content.ReadAsAsync<ContentItemsResponse>();
        Assert.Equal(0, none.TotalCount);
        Assert.Empty(none.Items);
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

    [Theory]
    [InlineData("draft")]
    [InlineData("latest")]
    public async Task ListAsync_CallerCanViewAllItems_PagesInTheDatabase(string status)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<ContentApiService>();
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Test")], nameof(StubIdentity)));
            foreach (var text in new[] { "Draft listing 1", "Draft listing 2" })
            {
                await service.SaveAsync(user, new ContentItem { ContentType = "BlogPost", DisplayText = text }, publish: false, allowCreate: true, allowUpdate: true);
            }
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveChangesAsync();

            var list = await service.ListAsync(user, "BlogPost", status, 0, 1);

            Assert.False(list.TotalCountIsEstimate);
            Assert.True(list.TotalCount >= 2);
            Assert.Single(list.Items);
        });
    }

    [Theory]
    [InlineData("modifiedUtc", true, ContentsOrder.Modified)]
    [InlineData("PUBLISHEDUTC", true, ContentsOrder.Published)]
    [InlineData("displayText", true, ContentsOrder.Title)]
    [InlineData(null, true, ContentsOrder.Modified)]
    [InlineData("createdUtc", false, ContentsOrder.Modified)]
    public void TryParseOrder_Value_ReturnsTheAdminOrder(string sort, bool expected, ContentsOrder expectedOrder)
    {
        Assert.Equal(expected, ContentApiService.TryParseOrder(sort, out var order));
        Assert.Equal(expectedOrder, order);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Zebra")]
    public async Task ListAsync_SortByDisplayText_OrdersAscending(string filter)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<ContentApiService>();
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Test")], nameof(StubIdentity)));
            foreach (var text in new[] { "Zebra 2", "Zebra 1" })
            {
                await service.SaveAsync(user, new ContentItem { ContentType = "BlogPost", DisplayText = text }, publish: true, allowCreate: true, allowUpdate: true);
            }
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveChangesAsync();

            var list = await service.ListAsync(user, "BlogPost", "published", 0, 100, filter, ContentsOrder.Title);
            var names = list.Items.Select(item => item.DisplayText).Where(text => text.StartsWith("Zebra", StringComparison.Ordinal)).ToArray();

            Assert.Equal(["Zebra 1", "Zebra 2"], names);
        });
    }

    [Theory]
    [InlineData("status:draft Findable", "published", 1)]
    [InlineData("Findable", "draft", 1)]
    [InlineData("Missing", "published", 0)]
    public async Task ListAsync_Filter_AppliesTheRequestStatusAndText(string filter, string status, int expected)
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        await context.UsingTenantScopeAsync(async scope =>
        {
            var service = scope.ServiceProvider.GetRequiredService<ContentApiService>();
            var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "Test")], nameof(StubIdentity)));
            await service.SaveAsync(user, new ContentItem { ContentType = "BlogPost", DisplayText = "Findable published" }, publish: true, allowCreate: true, allowUpdate: true);
            await service.SaveAsync(user, new ContentItem { ContentType = "BlogPost", DisplayText = "Findable draft" }, publish: false, allowCreate: true, allowUpdate: true);
            await scope.ServiceProvider.GetRequiredService<YesSql.ISession>().SaveChangesAsync();

            var list = await service.ListAsync(user, "BlogPost", status, 0, 100, filter);

            Assert.Equal(expected, list.TotalCount);
            Assert.All(list.Items, item => Assert.Contains(status, item.DisplayText, StringComparison.Ordinal));
        });
    }

    [Fact]
    public async Task ScanAuthorizedItemsAsync_PageIsFollowedByAnItem_StopsAndFlagsAnEstimate()
    {
        var loads = 0;

        var response = await ContentApiService.ScanAuthorizedItemsAsync(
            _ =>
            {
                loads++;
                return Task.FromResult<IEnumerable<ContentItem>>(Items(100));
            },
            _ => Task.FromResult(true),
            skip: 10,
            take: 20);

        Assert.Equal(1, loads);
        Assert.Equal(20, response.Items.Count);
        Assert.Equal(100, response.TotalCount);
        Assert.True(response.TotalCountIsEstimate);
    }

    [Fact]
    public async Task ScanAuthorizedItemsAsync_NothingAuthorized_StopsAtTheScanLimit()
    {
        var loads = 0;

        var response = await ContentApiService.ScanAuthorizedItemsAsync(
            _ =>
            {
                loads++;
                return Task.FromResult<IEnumerable<ContentItem>>(Items(100));
            },
            _ => Task.FromResult(false),
            skip: 0,
            take: 20,
            scanLimit: 300);

        Assert.Equal(3, loads);
        Assert.Empty(response.Items);
        Assert.True(response.TotalCountIsEstimate);
    }

    [Fact]
    public async Task ScanAuthorizedItemsAsync_StoreExhausted_ReturnsAnExactCount()
    {
        var offsets = new List<int>();

        var response = await ContentApiService.ScanAuthorizedItemsAsync(
            offset =>
            {
                offsets.Add(offset);
                return Task.FromResult<IEnumerable<ContentItem>>(Items(offset == 0 ? 100 : 40));
            },
            contentItem => Task.FromResult(contentItem.Owner == "me"),
            skip: 60,
            take: 20);

        Assert.Equal([0, 100], offsets);
        Assert.Equal(70, response.TotalCount);
        Assert.Equal(10, response.Items.Count);
        Assert.False(response.TotalCountIsEstimate);
        Assert.DoesNotContain("totalCountIsEstimate", JsonSerializer.Serialize(response), StringComparison.Ordinal);
    }

    private static ContentItem[] Items(int count)
        => Enumerable.Range(0, count).Select(index => new ContentItem { Owner = index % 2 == 0 ? "me" : "other" }).ToArray();

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
