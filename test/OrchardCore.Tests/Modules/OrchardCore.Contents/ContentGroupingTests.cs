using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Admin;
using OrchardCore.ContentLocalization.Indexes;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Indexes;
using YesSql.Provider.Sqlite;
using YesSql.Sql;
using ContentMigrations = OrchardCore.ContentManagement.Records.Migrations;
using LocalizationMigrations = OrchardCore.ContentLocalization.Records.Migrations;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public sealed class ContentGroupingTests : IAsyncLifetime
{
    private readonly string _filename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private IStore _store;

    public async ValueTask InitializeAsync()
    {
        _store = await StoreFactory.CreateAndInitializeAsync(new Configuration().UseSqLite($"Data Source={_filename};Pooling=False").SetTablePrefix("grouping_"));
        await using var session = _store.CreateSession();
        var schema = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync(TestContext.Current.CancellationToken));
        await new ContentMigrations(Mock.Of<IContentDefinitionManager>()) { SchemaBuilder = schema }.CreateAsync();
        Assert.Equal(5, await new LocalizationMigrations(Mock.Of<IContentDefinitionManager>()) { SchemaBuilder = schema }.CreateAsync());
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        _store.RegisterIndexes<ContentItemIndexProvider>();
        using var services = CreateServices(session);
        _store.RegisterIndexes(services.GetServices<IIndexProvider>().Append(services.GetRequiredService<LocalizedContentItemIndexProvider>()));
    }

    public ValueTask DisposeAsync()
    {
        _store.Dispose();
        File.Delete(_filename);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task GroupAsync_LocalizedVersionsAndPlainItems_PreservesAllRowsAndPageHeaders()
    {
        await using (var session = _store.CreateSession())
        {
            foreach (var (id, set, latest) in new (string, string, bool)[]
            {
                ("english", "shared", true),
                ("plain", null, true),
                ("french", "shared", true),
                ("old-english", "shared", false),
                ("other-plain", null, true),
            })
            {
                var item = new ContentItem { ContentItemId = id, ContentItemVersionId = id, ContentType = "Article", DisplayText = id, Latest = latest };
                if (set != null)
                {
                    item.Weld(new LocalizationPart { LocalizationSet = set, Culture = "en" });
                }

                await session.SaveAsync(item, cancellationToken: TestContext.Current.CancellationToken);
            }

            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var reader = _store.CreateSession();
        using var services = CreateServices(reader);
        var provider = Assert.Single(services.GetKeyedServices<IAdminListGroupingProvider>(new AdminListGroupingProviderKey("Contents")));
        var metadata = await reader.QueryIndex<ContentItemIndex>().OrderBy(index => index.Id).ListAsync(TestContext.Current.CancellationToken);
        var groups = await provider.GroupAsync(metadata, TestContext.Current.CancellationToken);

        Assert.Equal([3, 1, 1], groups.Select(group => group.TotalCount));
        Assert.Equal(["english", "french", "old-english", "plain", "other-plain"],
            groups.SelectMany(group => group.Source.Cast<ContentItemIndex>()).Select(index => index.DisplayText));
        var firstPage = AdminListGrouping.Page(groups, 0, 2);
        var secondPage = AdminListGrouping.Page(groups, 2, 2);
        Assert.Equal(firstPage[0].Title, secondPage[0].Title);
        Assert.Equal(3, secondPage[0].TotalCount);
        Assert.Equal(2, secondPage.Sum(group => group.Source.Cast<ContentItemIndex>().Count()));

        var filtered = await provider.GroupAsync(metadata.Where(index => index.Latest && index.DisplayText != "french").ToArray(), TestContext.Current.CancellationToken);
        Assert.Equal(3, filtered.Sum(group => group.TotalCount));
        Assert.Single(filtered[0].Source.Cast<ContentItemIndex>());
    }

    [Fact]
    public async Task SaveAsync_UnchangedHistoricalDocument_BackfillsMembership()
    {
        long id;
        await using (var session = _store.CreateSession())
        {
            var item = new ContentItem { ContentItemId = "historical", ContentItemVersionId = "historical", ContentType = "Article" };
            item.Weld(new LocalizationPart { LocalizationSet = "shared", Culture = "fr" });
            await session.SaveAsync(item, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
            id = item.Id;
        }

        await using (var session = _store.CreateSession())
        {
            var schema = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync(TestContext.Current.CancellationToken));
            await schema.DropTableAsync(nameof(LocalizationSetIndex));
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var session = _store.CreateSession())
        {
            var schema = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync(TestContext.Current.CancellationToken));
            Assert.Equal(5, await new LocalizationMigrations(Mock.Of<IContentDefinitionManager>()) { SchemaBuilder = schema }.UpdateFrom4Async());
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var session = _store.CreateSession())
        {
            var item = await session.GetAsync<ContentItem>(id);
            await session.SaveAsync(item, cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var reader = _store.CreateSession();
        var membership = Assert.Single(await reader.QueryIndex<LocalizationSetIndex>().ListAsync(TestContext.Current.CancellationToken));
        Assert.Equal(id, membership.DocumentId);
        Assert.Equal("shared", membership.LocalizationSet);
    }

    [Theory]
    [InlineData("culture:FR", "french")]
    [InlineData("culture:en", "english")]
    public async Task QueryIndexAsync_CultureFilter_DoesNotPullExcludedTranslationsIntoGroups(string filter, string expected)
    {
        await using (var session = _store.CreateSession())
        {
            foreach (var (name, culture) in new[] { ("english", "en"), ("french", "fr") })
            {
                var item = new ContentItem { ContentItemId = name, ContentItemVersionId = name, ContentType = "Article", DisplayText = name, Latest = true };
                item.Weld(new LocalizationPart { LocalizationSet = "shared", Culture = culture });
                await session.SaveAsync(item, cancellationToken: TestContext.Current.CancellationToken);
            }

            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var reader = _store.CreateSession();
        using var services = CreateServices(reader);
        var builder = new QueryEngineBuilder<ContentItem>();
        new LocalizationPartContentsAdminListFilterProvider().Build(builder);
        var queryService = new DefaultContentsAdminListQueryService(reader, services, [],
            Options.Create(new ContentsAdminListFilterOptions()), NullLogger<DefaultContentsAdminListQueryService>.Instance);
        var indexes = await queryService.QueryIndexAsync(
            new ContentOptionsViewModel { FilterResult = builder.Build().Parse(filter) },
            Mock.Of<IUpdateModel>(), TestContext.Current.CancellationToken);
        var provider = Assert.Single(services.GetKeyedServices<IAdminListGroupingProvider>(new AdminListGroupingProviderKey("Contents")));
        var group = Assert.Single(await provider.GroupAsync(indexes, TestContext.Current.CancellationToken));

        Assert.Equal(expected, Assert.Single(group.Source.Cast<ContentItemIndex>()).DisplayText);
        Assert.Equal(1, group.TotalCount);
    }

    private static ServiceProvider CreateServices(YesSql.ISession session)
    {
        var services = new ServiceCollection().AddLogging().AddLocalization().AddSingleton(session);
        new global::OrchardCore.ContentLocalization.Startup().ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
