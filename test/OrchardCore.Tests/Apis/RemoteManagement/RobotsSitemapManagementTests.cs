using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Sitemaps.Drivers;
using OrchardCore.Sitemaps.ViewModels;
using OrchardCore.Contents.Sitemaps;
using System.Text.Json.Nodes;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Documents;
using OrchardCore.Entities;
using OrchardCore.Localization;
using OrchardCore.Modules.FileProviders;
using OrchardCore.Modules.Services;
using OrchardCore.Seo;
using OrchardCore.Seo.Services;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class RobotsSitemapManagementTests
{
    [Fact]
    public async Task ExistingSourceEditorsRejectInvalidPriorityThroughSharedValidation()
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(value => value.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<CustomPathSitemapSourceViewModel>(), It.IsAny<string>(),
            It.IsAny<Expression<Func<CustomPathSitemapSourceViewModel, object>>[]>()))
            .Callback((CustomPathSitemapSourceViewModel model, string _, Expression<Func<CustomPathSitemapSourceViewModel, object>>[] _) =>
            {
                model.Path = "/valid";
                model.Priority = 11;
            }).ReturnsAsync(true);
        var context = new UpdateEditorContext(new Shape(), "", false, "", Mock.Of<IShapeFactory>(), Mock.Of<IZoneHolding>(), updater.Object);
        var custom = new CustomPathSitemapSourceDriver(new StringLocalizer<CustomPathSitemapSourceDriver>(new NullStringLocalizerFactory()));
        await custom.UpdateAsync(new CustomPathSitemapSource(), context);
        Assert.False(updater.Object.ModelState.IsValid);
        updater.Object.ModelState.Clear();
        updater.Setup(value => value.TryUpdateModelAsync(It.IsAny<ContentTypesSitemapSourceViewModel>(), It.IsAny<string>(),
            It.IsAny<Expression<Func<ContentTypesSitemapSourceViewModel, object>>[]>()))
            .Callback((ContentTypesSitemapSourceViewModel model, string _, Expression<Func<ContentTypesSitemapSourceViewModel, object>>[] _) => model.Priority = -1)
            .ReturnsAsync(true);
        var coordinator = new Mock<IRouteableContentTypeCoordinator>();
        coordinator.Setup(value => value.ListRoutableTypeDefinitionsAsync()).ReturnsAsync([]);
        await new ContentTypesSitemapSourceDriver(coordinator.Object).UpdateAsync(new ContentTypesSitemapSource(), context);
        Assert.False(updater.Object.ModelState.IsValid);
    }

    [Fact]
    public async Task RobotsSettingsChangeExistingPublicProvider_AndRetryIsUnchanged()
    {
        var site = new SiteSettings { IsReadOnly = false };
        var service = new Mock<ISiteService>();
        service.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(site);
        var files = new Mock<IStaticFileProvider>();
        files.Setup(value => value.GetFileInfo(SeoConstants.RobotsFileName)).Returns(new NotFoundFileInfo("robots.txt"));
        var provider = new RobotsSettingsSectionProvider(service.Object, files.Object);
        var patch = new JsonObject { ["allowAllAgents"] = true, ["disallowAdmin"] = true, ["additionalRules"] = "Disallow: /private" };
        Assert.True((await provider.UpdateAsync(patch)).Changed);
        Assert.False((await provider.UpdateAsync(patch)).Changed);
        var output = await new SiteSettingsRobotsProvider(service.Object, Options.Create(new AdminOptions { AdminUrlPrefix = "custom-admin" })).GetContentAsync();
        Assert.Contains("User-agent: *", output);
        Assert.Contains("Disallow: /custom-admin", output);
        Assert.Contains("Disallow: /private", output);
        Assert.NotEmpty((await provider.UpdateAsync(new JsonObject { ["additionalRules"] = "changed", ["unknown"] = true })).Errors);
        Assert.Equal("Disallow: /private", site.GetOrCreate<RobotsSettings>().AdditionalRules);
        Assert.True((await provider.UpdateAsync(new JsonObject { ["additionalRules"] = null })).Changed);
        Assert.Null(site.GetOrCreate<RobotsSettings>().AdditionalRules);
    }

    [Fact]
    public async Task ExplicitDefaultSitemapRobotsSettingCreatesTheAspectUsedByRuntime()
    {
        var site = new SiteSettings { IsReadOnly = false, BaseUrl = "https://example.test" };
        var service = new Mock<ISiteService>();
        service.Setup(value => value.LoadSiteSettingsAsync()).ReturnsAsync(site);
        service.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(site);
        var maps = Create();
        maps.Document.Sitemaps["map"] = new Sitemap { SitemapId = "map", Name = "Map", Path = "map.xml" };
        var provider = new SitemapsRobotsSettingsSectionProvider(service.Object);
        Assert.True((await provider.UpdateAsync(new JsonObject { ["includeSitemaps"] = true })).Changed);
        Assert.False((await provider.UpdateAsync(new JsonObject { ["includeSitemaps"] = true })).Changed);
        Assert.Contains("https://example.test/map.xml", await new SitemapsRobotsProvider(maps.Manager, service.Object).GetContentAsync());
    }

    [Fact]
    public async Task PhysicalRobotsFileReportsOwnershipAndPreventsIneffectiveUpdates()
    {
        var site = new Mock<ISiteService>();
        site.Setup(value => value.GetSiteSettingsAsync()).ReturnsAsync(new SiteSettings());
        var file = new Mock<IFileInfo>();
        file.SetupGet(value => value.Exists).Returns(true);
        var files = new Mock<IStaticFileProvider>();
        files.Setup(value => value.GetFileInfo(SeoConstants.RobotsFileName)).Returns(file.Object);
        var provider = new RobotsSettingsSectionProvider(site.Object, files.Object);
        Assert.True((await provider.GetAsync()).IsReadOnly);
        Assert.NotEmpty((await provider.UpdateAsync(new JsonObject { ["allowAllAgents"] = false })).Errors);
        site.Verify(value => value.LoadSiteSettingsAsync(), Times.Never);
    }

    [Fact]
    public async Task SitemapPathConflictsPreserveMetadataAndCacheIdentifier()
    {
        var fixture = Create();
        var existing = new Sitemap { SitemapId = "first", Name = "First", Path = "first.xml", Identifier = "original" };
        fixture.Document.Sitemaps["first"] = existing;
        fixture.Document.Sitemaps["second"] = new Sitemap { SitemapId = "second", Name = "Second", Path = "second.xml" };
        var service = Management(fixture.Manager);
        var failed = await service.SaveAsync(new SitemapDefinition { Name = "Changed", Path = "second.xml" }, "first");
        Assert.NotEmpty(failed.Errors);
        Assert.Equal("First", existing.Name);
        Assert.Equal("original", existing.Identifier);
        var updated = await service.SaveAsync(new SitemapDefinition { Name = "Changed", Path = "changed.xml", Enabled = false }, "first");
        Assert.True(updated.Changed);
        Assert.NotEqual("original", existing.Identifier);
        var identifier = existing.Identifier;
        Assert.False((await service.SaveAsync(new SitemapDefinition { Name = "Changed", Path = "changed.xml", Enabled = false }, "first")).Changed);
        Assert.Equal(identifier, existing.Identifier);
        fixture.Store.Verify(value => value.UpdateAsync(fixture.Document, null), Times.Once);
    }

    [Fact]
    public async Task ExistingManagerInvalidatesIndexesWhenChildChangesOrIsDeleted()
    {
        var fixture = Create();
        var child = new Sitemap { SitemapId = "child", Name = "Child", Path = "child.xml" };
        var index = new SitemapIndex { SitemapId = "index", Name = "Index", Path = "index.xml", Identifier = "before",
            SitemapSources = [new SitemapIndexSource { Id = "reference", ContainedSitemapIds = ["child"] }], };
        fixture.Document.Sitemaps["child"] = child;
        fixture.Document.Sitemaps["index"] = index;
        await fixture.Manager.UpdateSitemapAsync(child);
        Assert.NotEqual("before", index.Identifier);
        var afterUpdate = index.Identifier;
        await fixture.Manager.DeleteSitemapAsync("child");
        Assert.NotEqual(afterUpdate, index.Identifier);
    }

    [Fact]
    public async Task IndexRejectsMissingOrNestedReferencesWithoutWriting()
    {
        var fixture = Create();
        fixture.Document.Sitemaps["index"] = new SitemapIndex { SitemapId = "index", Name = "Index", Path = "index.xml" };
        var service = Management(fixture.Manager);
        foreach (var id in new[] { "missing", "index" })
        {
            Assert.NotEmpty((await service.SaveAsync(new SitemapDefinition { Name = "New", Path = "new.xml", Kind = "SitemapIndex", ContainedSitemapIds = [id] })).Errors);
        }
        fixture.Store.Verify(value => value.UpdateAsync(It.IsAny<SitemapDocument>(), null), Times.Never);
    }

    [Theory]
    [InlineData("{\"path\":\"/valid\",\"priority\":11}")]
    [InlineData("{\"path\":\"/bad path\"}")]
    [InlineData("{\"path\":\"/valid\",\"changeFrequency\":100}")]
    [InlineData("{\"path\":\"/valid\",\"unknown\":true}")]
    [InlineData("{\"path\":\"/valid\",\"id\":\"injected\"}")]
    public async Task InvalidSourceWritesDoNotMutateExistingSource(string json)
    {
        var fixture = Create();
        var source = new CustomPathSitemapSource { Id = "source", Path = "/old" };
        var sitemap = new Sitemap { SitemapId = "map", Name = "Map", Path = "map.xml", SitemapSources = [source] };
        fixture.Document.Sitemaps["map"] = sitemap;
        var service = Sources(fixture.Manager);
        Assert.NotEmpty((await service.SaveAsync("map", new SitemapSourceDefinition { Type = nameof(CustomPathSitemapSource), Configuration = JsonNode.Parse(json).AsObject() }, "source")).Errors);
        Assert.Same(source, Assert.Single(sitemap.SitemapSources));
        Assert.Equal("/old", source.Path);
        fixture.Store.Verify(value => value.UpdateAsync(It.IsAny<SitemapDocument>(), null), Times.Never);
    }

    [Fact]
    public void ThirdPartySourceExtensionsExposeIdentityOnly()
    {
        var response = SitemapSourceManagementService.Describe(new ExtendedCustomSource { Id = "extension", Path = "/private" });
        Assert.False(response.Supported);
        Assert.Equal("extension", response.Id);
        Assert.Null(response.Configuration);
    }

    private sealed class ExtendedCustomSource : CustomPathSitemapSource;

    [Fact]
    public async Task ValidSourceUpdateInvalidatesExistingSitemapCacheOnlyOnce()
    {
        var fixture = Create();
        var sitemap = new Sitemap { SitemapId = "map", Name = "Map", Path = "map.xml", Identifier = "old", SitemapSources = [new CustomPathSitemapSource { Id = "source", Path = "/old" }] };
        fixture.Document.Sitemaps["map"] = sitemap;
        var service = Sources(fixture.Manager);
        var input = new SitemapSourceDefinition { Type = nameof(CustomPathSitemapSource), Configuration = JsonNode.Parse("{\"path\":\"/new\",\"priority\":5,\"changeFrequency\":\"Daily\"}").AsObject() };
        Assert.True((await service.SaveAsync("map", input, "source")).Changed);
        Assert.NotEqual("old", sitemap.Identifier);
        Assert.False((await service.SaveAsync("map", input, "source")).Changed);
        fixture.Store.Verify(value => value.UpdateAsync(fixture.Document, null), Times.Once);
    }

    private static SitemapManagementService Management(ISitemapManager manager)
    {
        var slug = new Mock<ISlugService>();
        slug.Setup(value => value.Slugify(It.IsAny<string>())).Returns((string value) => value.ToLowerInvariant());
        return new SitemapManagementService(manager, new SitemapHelperService(slug.Object, manager,
            new StringLocalizer<SitemapHelperService>(new NullStringLocalizerFactory())), Mock.Of<ISitemapIdGenerator>());
    }
    private static SitemapSourceManagementService Sources(ISitemapManager manager)
    {
        var factory = new Mock<ISitemapSourceFactory>();
        factory.SetupGet(value => value.Name).Returns(nameof(CustomPathSitemapSource));
        return new SitemapSourceManagementService(manager, Mock.Of<ISitemapIdGenerator>(), [factory.Object], Mock.Of<IRouteableContentTypeCoordinator>());
    }
    private static (SitemapDocument Document, Mock<IDocumentManager<SitemapDocument>> Store, SitemapManager Manager) Create()
    {
        var document = new SitemapDocument { IsReadOnly = false };
        var store = new Mock<IDocumentManager<SitemapDocument>>();
        store.Setup(value => value.GetOrCreateMutableAsync(null)).ReturnsAsync(document);
        store.Setup(value => value.GetOrCreateImmutableAsync(null)).ReturnsAsync(document);
        return (document, store, new SitemapManager(store.Object));
    }
}
