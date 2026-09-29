using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.QuickNavigation;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Entities;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Security;
using OrchardCore.Settings;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Serialization;
using YesSql.Sql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentQuickNavigationTests : IAsyncLifetime
{
    private static readonly DateTime s_now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private SiteSettings _site = new();
    private readonly Mock<IContentManager> _contentManager = new();
    private readonly Mock<IAuthorizationService> _authorization = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();
    private IStore _store;
    private string _databasePath;

    public async ValueTask InitializeAsync()
    {
        _httpContextAccessor.SetupProperty(accessor => accessor.HttpContext, new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user")], "Test")),
            Request = { PathBase = "/tenant" },
        });
        _databasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(
            new Configuration().UseSqLite($"Data Source={_databasePath};Pooling=False"));
        var jsonOptions = new DocumentJsonSerializerOptions();
        new DocumentJsonSerializerOptionsConfiguration(Options.Create(new JsonDerivedTypesOptions())).Configure(jsonOptions);
        _store.Configuration.ContentSerializer = new DefaultContentJsonSerializer(Options.Create(jsonOptions));

        await using var session = _store.CreateSession();
        var builder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync());
        await builder.CreateMapIndexTableAsync<ContentItemIndex>(table => table
            .Column<string>("ContentItemId")
            .Column<string>("ContentItemVersionId")
            .Column<bool>("Latest")
            .Column<bool>("Published")
            .Column<string>("ContentType")
            .Column<DateTime>("ModifiedUtc", column => column.Nullable())
            .Column<DateTime>("PublishedUtc", column => column.Nullable())
            .Column<DateTime>("CreatedUtc", column => column.Nullable())
            .Column<string>("Owner", column => column.Nullable())
            .Column<string>("Author", column => column.Nullable())
            .Column<string>("DisplayText", column => column.Nullable()));
        await session.SaveChangesAsync();
        _store.RegisterIndexes<ContentItemIndexProvider>();

        _contentManager.Setup(manager => manager.LoadAsync(It.IsAny<ContentItem>()))
            .ReturnsAsync((ContentItem item) => item);
        _authorization.Setup(service => service.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync((ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements) =>
                user.Identity?.IsAuthenticated == true
                && resource is ContentItem item
                && item.Owner == user.FindFirstValue(ClaimTypes.NameIdentifier)
                && requirements.OfType<PermissionRequirement>().Single().Permission.Name == CommonPermissions.EditContent.Name
                    ? AuthorizationResult.Success()
                    : AuthorizationResult.Failed());
    }

    public ValueTask DisposeAsync()
    {
        _store?.Dispose();
        File.Delete(_databasePath);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GetEntryIds_DefaultSettings_ReturnsLast50LatestItemsInOrder()
    {
        var items = Enumerable.Range(0, 60)
            .Select(i => Item($"item-{i:D2}", s_now.AddMinutes(i)))
            .ToList();
        var olderVersion = Item("item-59", s_now.AddHours(2));
        olderVersion.Latest = false;
        olderVersion.Published = true;
        var removed = Item("removed", s_now.AddHours(3));
        removed.Latest = false;
        items.AddRange([olderVersion, removed]);
        await SaveAsync([.. items]);
        await using var session = _store.CreateSession();
        var source = CreateSource(session);

        var ids = (await source.GetEntryIdsAsync()).ToArray();

        Assert.Equal(50, new ContentQuickNavigationSettings().MaxItems);
        Assert.Equal(Enumerable.Range(10, 50).Reverse().Select(i => $"item-{i:D2}"), ids);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(55)]
    public async Task GetEntryIds_ConfiguredCount_UsesSiteSetting(int count)
    {
        await SaveAsync([.. Enumerable.Range(0, 60).Select(i => Item($"item-{i:D2}", s_now.AddMinutes(i)))]);
        _site.Put(new ContentQuickNavigationSettings { MaxItems = count });
        await using var session = _store.CreateSession();
        var source = CreateSource(session);

        var ids = (await source.GetEntryIdsAsync()).ToArray();

        Assert.Equal(count, ids.Length);
        Assert.Equal("item-59", ids[0]);
        Assert.Equal($"item-{60 - count:D2}", ids[^1]);
    }

    [Fact]
    public async Task GetEntryIds_ChangesAndTiedDates_RefreshesOrderAndRemovesOldEntries()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await SaveAsync(Item("a", s_now), Item("b", s_now), Item("c", s_now.AddDays(-1)));
        _site.Put(new ContentQuickNavigationSettings { MaxItems = 2 });
        await using (var session = _store.CreateSession())
        {
            Assert.Equal(["a", "b"], await CreateSource(session).GetEntryIdsAsync());
            var a = await session.Query<ContentItem, ContentItemIndex>(item => item.ContentItemId == "a").FirstOrDefaultAsync(cancellationToken);
            a.Latest = false;
            await session.SaveAsync(a, cancellationToken: cancellationToken);
            var c = await session.Query<ContentItem, ContentItemIndex>(item => item.ContentItemId == "c").FirstOrDefaultAsync(cancellationToken);
            c.ModifiedUtc = s_now.AddDays(1);
            await session.SaveAsync(c, cancellationToken: cancellationToken);
            await session.SaveChangesAsync(cancellationToken);
        }

        await using (var refreshedSession = _store.CreateSession())
        {
            Assert.Equal(["c", "b"], await CreateSource(refreshedSession).GetEntryIdsAsync());
        }

        // Site settings updates replace the read-only, cached snapshot.
        _site = new SiteSettings();
        _site.Put(new ContentQuickNavigationSettings { MaxItems = 1 });
        Assert.Equal(1, _site.GetOrCreate<ContentQuickNavigationSettings>().MaxItems);
        await using var nextRequestSession = _store.CreateSession();
        var source = CreateSource(nextRequestSession);
        Assert.Equal(["c"], await source.GetEntryIdsAsync());
        Assert.Null(await source.DisplayAsync("b"));
    }

    [Fact]
    public async Task Display_EditableDraft_UsesFullLoadedDisplayTextAndEditRoute()
    {
        var item = Item("draft", s_now);
        item.DisplayText = new string('a', 300);
        await SaveAsync(item);
        await using var session = _store.CreateSession();
        var source = CreateSource(session);
        await source.GetEntryIdsAsync();

        var result = await source.DisplayAsync("draft");

        Assert.Equal(item.DisplayText, result.Title);
        Assert.Equal(["Localized content"], result.Path);
        Assert.Equal("/tenant/manage/Contents/ContentItems/draft/Edit", result.Href);
        _contentManager.Verify(manager => manager.LoadAsync(It.Is<ContentItem>(item => item.ContentItemId == "draft")), Times.Once);
    }

    [Fact]
    public async Task Display_PermissionOrTitleMissing_DoesNotExposeDestination()
    {
        var denied = Item("denied", s_now);
        denied.Owner = "another-user";
        var empty = Item("empty", s_now);
        empty.DisplayText = " ";
        await SaveAsync(denied, empty, Item("allowed", s_now));
        await using var session = _store.CreateSession();
        var source = CreateSource(session);
        await source.GetEntryIdsAsync();

        Assert.Null(await source.DisplayAsync("denied"));
        Assert.Null(await source.DisplayAsync("empty"));
        Assert.Null(await source.DisplayAsync("missing"));
        Assert.NotNull(await source.DisplayAsync("allowed"));

        _httpContextAccessor.Object.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "another-user")], "Test"));

        Assert.Null(await source.DisplayAsync("allowed"));
        Assert.NotNull(await source.DisplayAsync("denied"));

        _httpContextAccessor.Object.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Null(await source.DisplayAsync("allowed"));
        Assert.Null(await source.DisplayAsync("denied"));
    }

    [Fact]
    public async Task Display_WithoutHttpContext_DoesNotExposeDestination()
    {
        await SaveAsync(Item("item", s_now));
        await using var session = _store.CreateSession();
        var source = CreateSource(session);
        await source.GetEntryIdsAsync();
        _httpContextAccessor.Object.HttpContext = null;

        Assert.Null(await source.DisplayAsync("item"));
        _authorization.Verify(service => service.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetEntryIds_InvalidCount_ReportsInvalidConfiguration(int count)
    {
        var settings = new ContentQuickNavigationSettings { MaxItems = count };
        _site.Put(settings);
        var validationResults = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(settings, new ValidationContext(settings), validationResults, true));
        Assert.Contains(nameof(ContentQuickNavigationSettings.MaxItems), Assert.Single(validationResults).MemberNames);
        await using var session = _store.CreateSession();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await CreateSource(session).GetEntryIdsAsync());
    }

    [Fact]
    public async Task Settings_WithoutManageAdminSettingsPermission_DoesNotDisplayOrUpdate()
    {
        var driver = new ContentQuickNavigationSettingsDisplayDriver(
            _httpContextAccessor.Object, _authorization.Object);
        var settings = new ContentQuickNavigationSettings();

        Assert.Null(await driver.EditAsync(_site, settings, new BuildEditorContext(new Shape(), "admin", false, "", null, null, null)));
        Assert.Null(await driver.UpdateAsync(_site, settings, new UpdateEditorContext(new Shape(), "admin", false, "", null, null, null)));
        Assert.Equal(50, settings.MaxItems);
        _authorization.Verify(service => service.AuthorizeAsync(
            It.IsAny<ClaimsPrincipal>(), null,
            It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                requirements.OfType<PermissionRequirement>().Single().Permission.Name == AdminPermissions.ManageAdminSettings.Name)), Times.Exactly(2));
    }

    private ContentItemNavigationSource CreateSource(ISession session)
    {
        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(_site);
        var localizer = new Mock<IStringLocalizer<ContentItemNavigationSource>>();
        localizer.Setup(localizer => localizer["Content"]).Returns(new LocalizedString("Content", "Localized content"));
        var linkGenerator = new Mock<LinkGenerator>();
        linkGenerator.Setup(generator => generator.GetPathByAddress(
            It.IsAny<HttpContext>(), It.IsAny<RouteValuesAddress>(), It.IsAny<RouteValueDictionary>(),
            It.IsAny<RouteValueDictionary>(), It.IsAny<PathString?>(), It.IsAny<FragmentString>(), It.IsAny<LinkOptions>()))
            .Returns((HttpContext context, RouteValuesAddress address, RouteValueDictionary values,
                RouteValueDictionary ambientValues, PathString? pathBase, FragmentString fragment, LinkOptions options) =>
            {
                Assert.Same(_httpContextAccessor.Object.HttpContext, context);
                Assert.Equal("EditContentItem", address.RouteName);
                return context.Request.PathBase + "/manage/Contents/ContentItems/" + values["contentItemId"] + "/Edit";
            });

        return new ContentItemNavigationSource(session, _contentManager.Object, siteService.Object,
            _authorization.Object, _httpContextAccessor.Object, linkGenerator.Object, localizer.Object);
    }

    private static ContentItem Item(string id, DateTime modifiedUtc) => new()
    {
        ContentItemId = id,
        ContentItemVersionId = Guid.NewGuid().ToString("N"),
        ContentType = "Article",
        DisplayText = id,
        Latest = true,
        ModifiedUtc = modifiedUtc,
        Owner = "user",
    };

    private async Task SaveAsync(params ContentItem[] items)
    {
        await using var session = _store.CreateSession();
        foreach (var item in items)
        {
            await session.SaveAsync(item);
        }

        await session.SaveChangesAsync();
    }
}
