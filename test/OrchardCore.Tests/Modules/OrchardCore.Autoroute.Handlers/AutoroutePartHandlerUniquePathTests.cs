using OrchardCore.Autoroute.Core.Indexes;
using OrchardCore.Autoroute.Handlers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using OrchardCore.Settings;
using YesSql.Indexes;
using YesSql.Provider.Sqlite;
using YesSql.Sql;

namespace OrchardCore.Tests.Modules.OrchardCore.Autoroute.Handlers;

public class AutoroutePartHandlerUniquePathTests : IAsyncLifetime
{
    private const string ContentItemId = "item";

    private readonly IndexQueryCounter _indexQueries = new();
    private IStore _store;
    private string _databasePath;

    public async ValueTask InitializeAsync()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        var configuration = new Configuration().UseSqLite($"Data Source={_databasePath};Pooling=False");
        configuration.Logger = _indexQueries;
        _store = await StoreFactory.CreateAndInitializeAsync(configuration);

        await using var session = _store.CreateSession();
        var builder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync());

        // The same table as the one the Autoroute migration creates.
        await builder.CreateMapIndexTableAsync<AutoroutePartIndex>(table => table
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<string>("ContainedContentItemId", c => c.WithLength(26))
            .Column<string>("JsonPath", c => c.Unlimited())
            .Column<string>("Path", c => c.WithLength(AutoroutePart.MaxPathLength))
            .Column<bool>("Published")
            .Column<bool>("Latest"));

        await session.SaveChangesAsync();

        _store.RegisterIndexes<RouteIndexProvider>();
    }

    public ValueTask DisposeAsync()
    {
        _store?.Dispose();
        File.Delete(_databasePath);

        return ValueTask.CompletedTask;
    }

    [Theory]
    [InlineData("blog/post", "blog/post-3")]
    [InlineData("blog/post-1", "blog/post-3")]
    [InlineData("blog/post-3", "blog/post-3")]
    [InlineData("blog/post-4", "blog/post-6")]
    public async Task GenerateUniqueAbsolutePathAsync_TakenNumbersWithGaps_ReturnsLowestFreeNumberFromTheStartingOne(string path, string expected)
    {
        // "blog/post-03" isn't the path of the third version.
        await SaveRoutesAsync(Taken("blog/post"), Taken("blog/post-1"), Taken("blog/post-2"), Taken("blog/post-03"), Taken("blog/post-4"), Taken("blog/post-5"));

        var (actual, _) = await GenerateUniqueAbsolutePathAsync(path);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GenerateUniqueAbsolutePathAsync_ManyTakenNumbers_QueriesTheIndexAsOftenAsForOne()
    {
        await SaveRoutesAsync(Taken("blog/post"), Taken("blog/post-1"));
        var (pathAfterOne, queriesAfterOne) = await GenerateUniqueAbsolutePathAsync("blog/post-1");

        await SaveRoutesAsync(Enumerable.Range(2, 299).Select(number => Taken($"blog/post-{number}")).ToArray());
        var (pathAfterMany, queriesAfterMany) = await GenerateUniqueAbsolutePathAsync("blog/post-1");

        Assert.Equal("blog/post-2", pathAfterOne);
        Assert.Equal("blog/post-301", pathAfterMany);
        Assert.Equal(queriesAfterOne, queriesAfterMany);
    }

    [Theory]
    [InlineData("blog/post", "blog/post-5")]
    [InlineData("/blog/post", "/blog/post-5")]
    public async Task GenerateUniqueAbsolutePathAsync_TakenWithLeadingOrTrailingSlash_SkipsThoseNumbers(string path, string expected)
    {
        // A path with two leading slashes isn't one of the four variants that conflict with "blog/post-5".
        await SaveRoutesAsync(Taken("blog/post-1"), Taken("/blog/post-2"), Taken("blog/post-3/"), Taken("/blog/post-4/"), Taken("//blog/post-5"));

        var (actual, _) = await GenerateUniqueAbsolutePathAsync(path);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(ContentItemId, null, true, true)]
    [InlineData("container", ContentItemId, true, true)]
    [InlineData("other", null, false, false)]
    public async Task GenerateUniqueAbsolutePathAsync_RouteOfTheItemItselfOrOfAnOldVersion_IsNotTaken(string contentItemId, string containedContentItemId, bool published, bool latest)
    {
        await SaveRoutesAsync(new RouteDocument
        {
            ContentItemId = contentItemId,
            ContainedContentItemId = containedContentItemId,
            Path = "blog/post-1",
            Published = published,
            Latest = latest,
        });

        var (actual, _) = await GenerateUniqueAbsolutePathAsync("blog/post");

        Assert.Equal("blog/post-1", actual);
    }

    [Theory]
    [InlineData("my_post", "myxpost-2")]
    [InlineData("50%off", "50-percent-off-2")]
    [InlineData("post[1]", "post1-2")]
    [InlineData("back\\slash", "backslash-2")]
    public async Task GenerateUniqueAbsolutePathAsync_PathWithLikeWildcard_OnlyTheSamePathIsTaken(string path, string lookalike)
    {
        await SaveRoutesAsync(Taken(path), Taken($"{path}-1"), Taken(lookalike));

        var (actual, _) = await GenerateUniqueAbsolutePathAsync(path);

        Assert.Equal($"{path}-2", actual);
    }

    [Fact]
    public async Task GenerateUniqueAbsolutePathAsync_NearTheMaximumLength_ShortensThePathForLongerNumbers()
    {
        // "-9" still fits after this path, "-10" doesn't.
        var unversionedPath = new string('a', AutoroutePart.MaxPathLength - 2);
        var shortenedPath = unversionedPath[..^1];

        await SaveRoutesAsync(Taken($"{unversionedPath}-9"), Taken($"{shortenedPath}-10"), Taken($"{unversionedPath}-11"));

        var (afterNine, _) = await GenerateUniqueAbsolutePathAsync($"{unversionedPath}-9");
        var (fromMaximumLength, _) = await GenerateUniqueAbsolutePathAsync(new string('a', AutoroutePart.MaxPathLength));

        Assert.Equal($"{shortenedPath}-11", afterNine);
        Assert.Equal($"{unversionedPath}-1", fromMaximumLength);
    }

    [Fact]
    public async Task GenerateUniqueAbsolutePathAsync_TakenWithDifferentCase_SkipsThatNumber()
    {
        // Routes are matched case-insensitively, and so are paths by SQL Server's default collation, and by SQLite here.
        await SaveRoutesAsync(Taken("blog/post-1"), Taken("Blog/Post-2"));

        var (actual, _) = await GenerateUniqueAbsolutePathAsync("blog/post-1");

        Assert.Equal("blog/post-3", actual);
    }

    [Fact]
    public async Task GenerateUniqueAbsolutePathAsync_PathTheDatabaseConsidersEqual_SkipsThatNumber()
    {
        // SQL Server ignores trailing spaces when it compares strings, and some collations ignore accents. SQLite's
        // RTRIM collation stands in for them here: "blog/post-1 " is the same path as "blog/post-1" to the database.
        await RecreateIndexTableAsync(pathCollation: "RTRIM");
        await SaveRoutesAsync(Taken("blog/post-1 "));

        var (actual, _) = await GenerateUniqueAbsolutePathAsync("blog/post");

        Assert.Equal("blog/post-2", actual);
    }

    private static RouteDocument Taken(string path)
        => new() { ContentItemId = "other", Path = path, Published = true, Latest = true };

    private async Task SaveRoutesAsync(params RouteDocument[] routes)
    {
        await using var session = _store.CreateSession();

        foreach (var route in routes)
        {
            await session.SaveAsync(route);
        }

        await session.SaveChangesAsync();
    }

    private async Task<(string Path, int IndexQueries)> GenerateUniqueAbsolutePathAsync(string path)
    {
        await using var session = _store.CreateSession();

        var handler = new AutoroutePartHandler(
            Mock.Of<IAutorouteEntries>(),
            Options.Create(new AutorouteOptions()),
            Mock.Of<ILiquidTemplateManager>(),
            Mock.Of<IContentDefinitionManager>(),
            Mock.Of<ISiteService>(),
            Mock.Of<ITagCache>(),
            session,
            Mock.Of<IServiceProvider>(),
            Mock.Of<IStringLocalizer<AutoroutePartHandler>>());

        var indexQueriesBefore = _indexQueries.Count;
        var uniquePath = await handler.GenerateUniqueAbsolutePathAsync(path, ContentItemId);

        return (uniquePath, _indexQueries.Count - indexQueriesBefore);
    }

    private async Task RecreateIndexTableAsync(string pathCollation)
    {
        await using var session = _store.CreateSession();
        var transaction = await session.BeginTransactionAsync();

        await using var command = transaction.Connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            "DROP TABLE AutoroutePartIndex; " +
            "CREATE TABLE AutoroutePartIndex (Id INTEGER PRIMARY KEY AUTOINCREMENT, DocumentId BIGINT, ContentItemId TEXT, " +
            $"ContainedContentItemId TEXT, JsonPath TEXT, Path TEXT COLLATE {pathCollation}, Published BOOLEAN, Latest BOOLEAN);";
        await command.ExecuteNonQueryAsync();

        await session.SaveChangesAsync();
    }

    public sealed class RouteDocument
    {
        public string ContentItemId { get; set; }

        public string ContainedContentItemId { get; set; }

        public string Path { get; set; }

        public bool Published { get; set; }

        public bool Latest { get; set; }
    }

    private sealed class RouteIndexProvider : IndexProvider<RouteDocument>
    {
        public override void Describe(DescribeContext<RouteDocument> context)
        {
            context.For<AutoroutePartIndex>()
                .Map(route => new AutoroutePartIndex
                {
                    ContentItemId = route.ContentItemId,
                    ContainedContentItemId = route.ContainedContentItemId,
                    Path = route.Path,
                    Published = route.Published,
                    Latest = route.Latest,
                });
        }
    }

    // YesSql logs the SQL of each query at the debug level.
    private sealed class IndexQueryCounter : ILogger
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public IDisposable BeginScope<TState>(TState state)
            => null;

        public bool IsEnabled(LogLevel logLevel)
            => logLevel == LogLevel.Debug;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var sql = formatter(state, exception);

            if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && sql.Contains("AutoroutePartIndex", StringComparison.Ordinal))
            {
                Interlocked.Increment(ref _count);
            }
        }
    }
}
