using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Services;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Data;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Taxonomies.Indexing;
using OrchardCore.Taxonomies.Services;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Taxonomies.ViewModels;
using YesSql;
using YesSql.Filters.Query;
using YesSql.Indexes;
using YesSql.Provider.Sqlite;
using YesSql.Sql;
using ContentMigrations = OrchardCore.ContentManagement.Records.Migrations;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

/// <summary>
/// Runs the filters of the contents admin list on a database, as a list only stops early by not grouping the matches by document, which
/// must not make a content item appear twice or the pages disagree with the count. The list doesn't stop early on SQLite, so the tenant
/// says it uses another database, to run that SQL on SQLite anyway.
/// </summary>
public class ContentsAdminListDuplicatesTests : IAsyncLifetime
{
    private const string TaxonomyId = "taxonomy";

    // From the most recently modified: A and C have several terms of the taxonomy, so they have several records in TaxonomyIndex.
    private static readonly (string Name, string[] Terms)[] s_items =
    [
        ("A", ["t1", "t2"]),
        ("B", ["t1"]),
        ("C", ["t1", "t2", "t3"]),
        ("D", ["t2"]),
        ("E", []),
    ];

    private readonly QueryLogger _logger = new();
    private IStore _store;
    private string _tempFilename;

    public async ValueTask InitializeAsync()
    {
        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var configuration = new Configuration().UseSqLite($"Data Source={_tempFilename};Cache=Shared");
        configuration.Logger = _logger;
        _store = await StoreFactory.CreateAndInitializeAsync(configuration);

        await using (var session = _store.CreateSession())
        {
            var schemaBuilder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync());
            await new ContentMigrations(Mock.Of<IContentDefinitionManager>()) { SchemaBuilder = schemaBuilder }.CreateAsync();
            await schemaBuilder.CreateMapIndexTableAsync<TaxonomyIndex>(table => table
                .Column<string>("TaxonomyContentItemId", column => column.WithLength(26))
                .Column<string>("ContentItemId", column => column.WithLength(26))
                .Column<string>("ContentType", column => column.WithLength(255))
                .Column<string>("ContentPart", column => column.WithLength(255))
                .Column<string>("ContentField", column => column.WithLength(255))
                .Column<string>("TermContentItemId", column => column.WithLength(26))
                .Column<bool>("Published")
                .Column<bool>("Latest"));
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        _store.RegisterIndexes<ContentItemIndexProvider>();
        _store.RegisterIndexes<TermsIndexProvider>();

        await using (var session = _store.CreateSession())
        {
            var modified = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            for (var i = 0; i < s_items.Length; i++)
            {
                await session.SaveAsync(new ContentItem
                {
                    ContentItemId = s_items[i].Name,
                    ContentItemVersionId = s_items[i].Name,
                    ContentType = "Article",
                    DisplayText = s_items[i].Name,
                    Latest = true,
                    Published = true,
                    ModifiedUtc = modified.AddDays(-i),
                }, cancellationToken: TestContext.Current.CancellationToken);
            }

            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }
    }

    public ValueTask DisposeAsync()
    {
        _store?.Dispose();
        _store = null;

        if (_tempFilename != null && File.Exists(_tempFilename))
        {
            try
            {
                File.Delete(_tempFilename);
            }
            catch
            {
            }
        }

        return ValueTask.CompletedTask;
    }

    [Theory]
    [InlineData("")]
    [InlineData("sort:Modified")]
    [InlineData("status:Latest")]
    [InlineData("status:Published sort:Modified")]
    [InlineData("type:Article")]
    public async Task QueryAsync_ContentItemIndexOnly_StopsEarly(string filter)
    {
        var result = await ListAsync(filter);

        Assert.False(result.Grouped);
        Assert.Equal(5, result.Count);
        Assert.Equal(["A", "B", "C", "D", "E"], result.Items);
    }

    [Fact]
    public async Task QueryAsync_SameModifiedDate_OrdersTheLastSavedFirst()
    {
        await using (var session = _store.CreateSession())
        {
            var modified = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            foreach (var name in new[] { "X", "Y", "Z" })
            {
                await session.SaveAsync(new ContentItem
                {
                    ContentItemId = name,
                    ContentItemVersionId = name,
                    ContentType = "Article",
                    Latest = true,
                    ModifiedUtc = modified,
                }, cancellationToken: TestContext.Current.CancellationToken);
            }

            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var result = await ListAsync(string.Empty);

        Assert.False(result.Grouped);
        Assert.Equal(["Z", "Y", "X", "A", "B", "C", "D", "E"], result.Items);
    }

    [Fact]
    public async Task QueryAsync_OnSqlite_DeDuplicates()
    {
        var result = await ListAsync(string.Empty, databaseProvider: DatabaseProviderValue.Sqlite);

        Assert.True(result.Grouped);
        Assert.Equal(5, result.Count);
        Assert.Equal(["A", "B", "C", "D", "E"], result.Items);
    }

    [Theory]
    [InlineData("sort:Title")]
    [InlineData("sort:Created")]
    [InlineData("sort:Published")]
    [InlineData("A")]
    public async Task QueryAsync_OtherOrderOrSearch_DeDuplicates(string filter)
    {
        var result = await ListAsync(filter);

        Assert.True(result.Grouped);
        Assert.Equal(result.Count, result.Items.Count);
    }

    [Theory]
    [InlineData("Taxonomy:" + TaxonomyId, new[] { "A", "B", "C", "D" })]
    [InlineData("Term:t1", new[] { "A", "B", "C" })]
    [InlineData("Term:t3", new[] { "C" })]
    public async Task QueryAsync_TaxonomyFilter_ReturnsEachItemOnce(string selected, string[] expected)
    {
        var result = await ListAsync(string.Empty, selectedTaxonomy: selected);

        Assert.Equal(expected.Length, result.Count);
        Assert.Equal(expected, result.Items);
        Assert.True(result.Grouped);
    }

    [Fact]
    public async Task QueryAsync_TaxonomyFilterWithoutSelection_StopsEarly()
    {
        var result = await ListAsync(string.Empty, selectedTaxonomy: string.Empty);

        Assert.False(result.Grouped);
        Assert.Equal(["A", "B", "C", "D", "E"], result.Items);
    }

    [Fact]
    public async Task QueryAsync_FilterNotDeclaringDuplicates_ReturnsEachItemOnce()
    {
        var result = await ListAsync(string.Empty, filters: [new JoiningFilter()]);

        Assert.Equal(4, result.Count);
        Assert.Equal(["A", "B", "C", "D"], result.Items);
        Assert.True(result.Grouped);
    }

    [Fact]
    public async Task QueryAsync_CustomTerm_ReturnsEachItemOnce()
    {
        var result = await ListAsync("term:t1", configure: builder => builder
            .WithNamedTerm("term", term => term
                .OneCondition((val, query) => query.With<TaxonomyIndex>(x => x.TermContentItemId == val))));

        Assert.Equal(["A", "B", "C"], result.Items);
        Assert.True(result.Grouped);
    }

    [Fact]
    public async Task QueryAsync_CustomAlwaysRunTerm_ReturnsEachItemOnce()
    {
        var result = await ListAsync(string.Empty, configure: builder => builder
            .WithNamedTerm("term", term => term
                .OneCondition((val, query) => query.With<TaxonomyIndex>(x => x.TaxonomyContentItemId == TaxonomyId))
                .AlwaysRun()));

        Assert.Equal(["A", "B", "C", "D"], result.Items);
        Assert.True(result.Grouped);
    }

    private async Task<ListResult> ListAsync(
        string filter,
        string selectedTaxonomy = null,
        IContentsAdminListFilter[] filters = null,
        Action<QueryEngineBuilder<ContentItem>> configure = null,
        string databaseProvider = DatabaseProviderValue.Postgres)
    {
        var builder = new QueryEngineBuilder<ContentItem>();
        new DefaultContentsAdminListFilterProvider().Build(builder);
        configure?.Invoke(builder);
        var parser = builder.Build();

        var updater = new Mock<IUpdateModel>();
        updater
            .Setup(value => value.TryUpdateModelAsync(It.IsAny<TaxonomyContentsAdminFilterViewModel>(), "Taxonomy" + TaxonomyId))
            .Callback((TaxonomyContentsAdminFilterViewModel viewModel, string _) => viewModel.SelectedContentItemId = selectedTaxonomy)
            .ReturnsAsync(true);

        var site = new SiteSettings();
        site.Put(new TaxonomyContentsAdminListSettings { TaxonomyContentItemIds = [TaxonomyId] });

        var siteService = new Mock<ISiteService>();
        siteService.Setup(service => service.GetSiteSettingsAsync()).ReturnsAsync(site);

        using var services = CreateServices();

        // A query can only run once, the controller runs one for the count, and one for the page.
        async Task<IQuery<ContentItem>> QueryAsync(YesSql.ISession session)
        {
            List<IContentsAdminListFilter> listFilters = [.. filters ?? []];
            if (selectedTaxonomy != null)
            {
                listFilters.Add(new TaxonomyContentsAdminListFilter(siteService.Object));
            }

            var queryService = new DefaultContentsAdminListQueryService(
                session,
                services,
                listFilters,
                Options.Create(new ContentsAdminListFilterOptions()),
                new ShellSettings { ["DatabaseProvider"] = databaseProvider },
                NullLogger<DefaultContentsAdminListQueryService>.Instance);

            return await queryService.QueryAsync(new ContentOptionsViewModel { FilterResult = parser.Parse(filter) }, updater.Object);
        }

        int count;
        await using (var session = _store.CreateSession())
        {
            count = await (await QueryAsync(session)).CountAsync(TestContext.Current.CancellationToken);
        }

        // Two items at a time, as a duplicate would also move the items after it to another page.
        var items = new List<string>();
        for (var page = 0; ; page++)
        {
            await using var session = _store.CreateSession();
            var query = await QueryAsync(session);
            var pageOfItems = await query.Skip(page * 2).Take(2).ListAsync(TestContext.Current.CancellationToken);

            if (pageOfItems.Count == 0)
            {
                return new ListResult(count, items, _logger.Query.Contains("GROUP BY", StringComparison.Ordinal));
            }

            items.AddRange(pageOfItems.Select(item => item.ContentItemId));
        }
    }

    private static ServiceProvider CreateServices()
    {
        var authorizationService = new Mock<IAuthorizationService>();
        authorizationService
            .Setup(service => service.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var article = new ContentTypeDefinitionBuilder()
            .WithName("Article")
            .WithDisplayName("Article")
            .Listable()
            .Build();

        var definitionManager = new Mock<IContentDefinitionManager>();
        definitionManager.Setup(manager => manager.ListTypeDefinitionsAsync()).ReturnsAsync(new[] { article }.AsEnumerable());
        definitionManager.Setup(manager => manager.GetTypeDefinitionAsync("Article")).ReturnsAsync(article);

        return new ServiceCollection()
            .AddSingleton<IHttpContextAccessor>(new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "user")], "Test")),
                },
            })
            .AddSingleton(authorizationService.Object)
            .AddSingleton(definitionManager.Object)
            .BuildServiceProvider();
    }

    private sealed record ListResult(int Count, List<string> Items, bool Grouped);

    // A module's filter, which doesn't say whether it can return duplicates.
    private sealed class JoiningFilter : IContentsAdminListFilter
    {
        public Task FilterAsync(ContentOptionsViewModel model, IQuery<ContentItem> query, IUpdateModel updater)
        {
            query.With<TaxonomyIndex>(x => x.TaxonomyContentItemId == TaxonomyId);

            return Task.CompletedTask;
        }
    }

    private sealed class TermsIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
            => context.For<TaxonomyIndex>()
                .Map(contentItem => s_items
                    .Where(item => item.Name == contentItem.ContentItemId)
                    .SelectMany(item => item.Terms)
                    .Select(term => new TaxonomyIndex
                    {
                        TaxonomyContentItemId = TaxonomyId,
                        ContentItemId = contentItem.ContentItemId,
                        ContentType = contentItem.ContentType,
                        ContentPart = "Part",
                        ContentField = "Field",
                        TermContentItemId = term,
                        Published = contentItem.Published,
                        Latest = contentItem.Latest,
                    }));
    }

    private sealed class QueryLogger : ILogger
    {
        public string Query { get; private set; } = string.Empty;

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Debug;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Query = formatter(state, exception);
    }
}
