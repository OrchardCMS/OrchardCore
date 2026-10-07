using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;
using YesSql.Provider.Sqlite;
using YesSql.Serialization;
using YesSql.Sql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;
using WorkflowMigrations = OrchardCore.Workflows.Migrations;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

/// <summary>
/// A SQLite database with the Workflows tables, created by the real migrations, and the real workflow type and
/// version stores on top of it.
/// </summary>
internal sealed class VersioningTestDatabase : IAsyncDisposable
{
    public static readonly DateTime Now = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly List<ISession> _sessions = [];
    private readonly string _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

    private VersioningTestDatabase()
    {
    }

    public IStore Store { get; private set; }

    public DocumentJsonSerializerOptions JsonOptions { get; private set; }

    public static async Task<VersioningTestDatabase> CreateAsync()
    {
        var database = new VersioningTestDatabase();
        database.Store = await StoreFactory.CreateAndInitializeAsync(new Configuration().UseSqLite($"Data Source={database._tempFilename};Cache=Shared"));

        var derivedOptions = new Mock<IOptions<JsonDerivedTypesOptions>>();
        derivedOptions.Setup(x => x.Value).Returns(new JsonDerivedTypesOptions());
        database.JsonOptions = new DocumentJsonSerializerOptions();
        new DocumentJsonSerializerOptionsConfiguration(derivedOptions.Object).Configure(database.JsonOptions);
        database.Store.Configuration.ContentSerializer = new DefaultContentJsonSerializer(Options.Create(database.JsonOptions));
        await database.Store.InitializeCollectionAsync(WorkflowExecutionRecord.Collection);

        // The deferred work of the migrations needs a shell scope, so it doesn't run here.
        await using (var session = database.Store.CreateSession())
        {
            var migrations = new WorkflowMigrations { SchemaBuilder = new SchemaBuilder(database.Store.Configuration, await session.BeginTransactionAsync()) };
            await migrations.CreateAsync();
            await migrations.UpdateFrom4Async();
            await migrations.UpdateFrom5Async();
            await migrations.UpdateFrom6Async();
            await session.SaveChangesAsync();
        }

        database.Store.RegisterIndexes<WorkflowTypeIndexProvider>();
        database.Store.RegisterIndexes<WorkflowIndexProvider>();
        database.Store.RegisterIndexes<WorkflowTypeVersionIndexProvider>();
        database.Store.RegisterIndexes<WorkflowExecutionRecordIndexProvider>();

        return database;
    }

    /// <summary>
    /// Returns a new session and the stores using it. Each call is a separate unit of work.
    /// </summary>
    public (ISession Session, WorkflowTypeVersionStore Versions, WorkflowTypeStore Types) CreateStores(int maxVersionCount = 0)
    {
        var session = Store.CreateSession();
        _sessions.Add(session);

        var idGenerator = new Mock<IIdGenerator>();
        idGenerator.Setup(x => x.GenerateUniqueId()).Returns(() => IdGenerator.GenerateId());

        var versions = new WorkflowTypeVersionStore(
            session,
            idGenerator.Object,
            Mock.Of<IClock>(x => x.UtcNow == Now),
            Mock.Of<IHttpContextAccessor>(),
            Options.Create(JsonOptions),
            Options.Create(new WorkflowVersionOptions { MaxCount = maxVersionCount }),
            NullLogger<WorkflowTypeVersionStore>.Instance);

        var types = new WorkflowTypeStore(session, versions, CreateJournal(session), [], NullLogger<WorkflowTypeStore>.Instance);

        return (session, versions, types);
    }

    /// <summary>
    /// Returns the journal over a session.
    /// </summary>
    public static WorkflowExecutionJournal CreateJournal(ISession session, int maxRecordsPerInstance = 1000)
        => new(session, Options.Create(new WorkflowJournalOptions { MaxRecordsPerInstance = maxRecordsPerInstance }));

    public async ValueTask DisposeAsync()
    {
        foreach (var session in _sessions)
        {
            await session.DisposeAsync();
        }

        Store?.Dispose();

        // Pooled connections keep the database file open.
        SqliteConnection.ClearAllPools();

        try
        {
            File.Delete(_tempFilename);
        }
        catch (IOException)
        {
            // A temporary file left behind doesn't affect other tests.
        }
    }
}
