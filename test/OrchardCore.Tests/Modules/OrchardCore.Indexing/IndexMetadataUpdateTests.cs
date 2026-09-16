using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Core.Models;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Indexing.Models;
using OrchardCore.Lucene;
using OrchardCore.Lucene.Core.Handlers;
using OrchardCore.Lucene.Models;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Indexing;

public class IndexMetadataUpdateTests
{
    [Fact]
    public async Task Recipe_Update_AppliesContentAndLuceneSettingsThroughExistingHandlers()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var profile = CreateProfile();
        var store = new Mock<IIndexProfileStore>();
        store.Setup(value => value.FindByIdAsync("id")).ReturnsAsync(profile);
        var manager = CreateManager(store.Object, services);
        var recipe = new CreateOrUpdateIndexProfileStep(manager, Options.Create(new IndexingOptions()), new IndexProfileManagementService(manager, services),
            Mock.Of<IStringLocalizer<CreateOrUpdateIndexProfileStep>>());
        var context = new RecipeExecutionContext
        {
            Name = CreateOrUpdateIndexProfileStep.StepKey,
            Step = JsonNode.Parse("""
                {"Indexes":[{"Id":"id","AnalyzerName":"KeywordAnalyzer","StoreSourceData":true,
                "IndexLatest":true,"Culture":"fr","IndexedContentTypes":["Article"]}]}
                """).AsObject(),
        };

        await recipe.ExecuteAsync(context);

        Assert.Empty(context.Errors);
        var content = ContentMetadata(profile);
        Assert.Equal(["Article"], content.IndexedContentTypes);
        Assert.True(content.IndexLatest);
        Assert.Equal("fr", content.Culture);
        var lucene = LuceneMetadata(profile);
        Assert.Equal("KeywordAnalyzer", lucene.AnalyzerName);
        Assert.True(lucene.StoreSourceData);
        Assert.NotNull(lucene.IndexMappings);
        Assert.Equal("retained", profile.Properties["Extension"]["Value"].GetValue<string>());
        store.Verify(value => value.UpdateAsync(profile), Times.Once);
    }

    [Fact]
    public async Task New_AppliesContentAndLuceneSettingsAndBuildsMappings()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var manager = CreateManager(Mock.Of<IIndexProfileStore>(), services);

        var profile = await manager.NewAsync(LuceneConstants.ProviderName, IndexingConstants.ContentsIndexSource,
            JsonNode.Parse("""
                {"AnalyzerName":"KeywordAnalyzer","StoreSourceData":true,"IndexLatest":true,
                "Culture":"fr","IndexedContentTypes":["Article"]}
                """));

        Assert.Equal(["Article"], ContentMetadata(profile).IndexedContentTypes);
        Assert.Equal("fr", ContentMetadata(profile).Culture);
        Assert.True(ContentMetadata(profile).IndexLatest);
        Assert.Equal("KeywordAnalyzer", LuceneMetadata(profile).AnalyzerName);
        Assert.True(LuceneMetadata(profile).StoreSourceData);
        Assert.NotNull(LuceneMetadata(profile).IndexMappings);
    }

    [Fact]
    public async Task Update_WithoutJson_PreservesEditorSettings()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var profile = CreateProfile();
        var manager = CreateManager(Mock.Of<IIndexProfileStore>(), services);

        await manager.UpdateAsync(profile);

        Assert.Equal(["Page"], ContentMetadata(profile).IndexedContentTypes);
        Assert.Equal("en", ContentMetadata(profile).Culture);
        Assert.Equal("StandardAnalyzer", LuceneMetadata(profile).AnalyzerName);
        Assert.False(LuceneMetadata(profile).StoreSourceData);
        Assert.NotNull(LuceneMetadata(profile).IndexMappings);
    }

    [Fact]
    public async Task Update_EmptyContentTypes_RejectsAndRestoresExistingMetadata()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var profile = CreateProfile();
        var store = new Mock<IIndexProfileStore>();
        var manager = CreateManager(store.Object, services);

        await Assert.ThrowsAsync<IndexProfileValidationException>(() => manager.UpdateAsync(profile,
            new JsonObject { ["IndexedContentTypes"] = new JsonArray() }).AsTask());

        Assert.Equal(["Page"], ContentMetadata(profile).IndexedContentTypes);
        store.Verify(value => value.UpdateAsync(It.IsAny<IndexProfile>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_QueryFieldsReplaceInsteadOfAppend_AndLegacyVersionIsPreserved(bool clear)
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var profile = CreateProfile();
        profile.Put(new LuceneIndexDefaultQueryMetadata { DefaultSearchFields = ["OldField", "OtherField"] });
        var manager = CreateManager(Mock.Of<IIndexProfileStore>(), services);
        var fields = clear ? new JsonArray() : new JsonArray("Title");
        var data = new JsonObject { ["DefaultSearchFields"] = fields, ["DefaultVersion"] = "LUCENE_30" };

        await manager.UpdateAsync(profile, data);
        await manager.UpdateAsync(profile, data);

        Assert.True(profile.TryGet<LuceneIndexDefaultQueryMetadata>(out var query));
        Assert.Equal(clear ? [] : new[] { "Title" }, query.DefaultSearchFields);
        Assert.Equal("LUCENE_30", query.DefaultVersion.ToString());
    }

    private static ContentIndexMetadata ContentMetadata(IndexProfile profile)
    {
        Assert.True(profile.TryGet<ContentIndexMetadata>(out var metadata));
        return metadata;
    }

    private static LuceneIndexMetadata LuceneMetadata(IndexProfile profile)
    {
        Assert.True(profile.TryGet<LuceneIndexMetadata>(out var metadata));
        return metadata;
    }

    private static IndexProfile CreateProfile()
    {
        var profile = new IndexProfile { Id = "id", ProviderName = LuceneConstants.ProviderName, Type = IndexingConstants.ContentsIndexSource };
        profile.Put(new ContentIndexMetadata { IndexedContentTypes = ["Page"], Culture = "en" });
        profile.Put(new LuceneIndexMetadata { AnalyzerName = "StandardAnalyzer" });
        profile.Properties["Extension"] = new JsonObject { ["Value"] = "retained" };
        return profile;
    }

    private static DefaultIndexProfileManager CreateManager(IIndexProfileStore store, IServiceProvider services)
    {
        var content = new Mock<IContentManager>();
        content.Setup(value => value.NewAsync(It.IsAny<string>())).ReturnsAsync(new ContentItem { ContentItemId = "content-id", ContentItemVersionId = "version-id" });
        var localizer = new Mock<IStringLocalizer<ContentIndexProfileHandler>>();
        localizer.Setup(value => value[It.IsAny<string>()]).Returns((string value) => new LocalizedString(value, value));
        return new DefaultIndexProfileManager(store,
            [new ContentIndexProfileHandler(services, null, localizer.Object), new LuceneIndexProfileHandler(),
                new LuceneContentIndexProfileHandler(content.Object, [], NullLogger<LuceneContentIndexProfileHandler>.Instance)],
            NullLogger<DefaultIndexProfileManager>.Instance);
    }
}
