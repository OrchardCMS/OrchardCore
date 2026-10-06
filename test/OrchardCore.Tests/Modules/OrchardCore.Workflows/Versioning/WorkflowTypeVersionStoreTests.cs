using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.Extensions;
using OrchardCore.Json;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;
using OrchardCore.Workflows.Deployment;
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

public sealed class WorkflowTypeVersionStoreTests : IAsyncLifetime
{
    private static readonly DateTime s_now = new(2026, 10, 6, 9, 0, 0, DateTimeKind.Utc);

    private readonly List<ISession> _sessions = [];
    private DocumentJsonSerializerOptions _jsonOptions;
    private IStore _store;
    private string _tempFilename;

    public async ValueTask InitializeAsync()
    {
        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(new Configuration().UseSqLite($"Data Source={_tempFilename};Cache=Shared"));

        var derivedOptions = new Mock<IOptions<JsonDerivedTypesOptions>>();
        derivedOptions.Setup(x => x.Value).Returns(new JsonDerivedTypesOptions());
        _jsonOptions = new DocumentJsonSerializerOptions();
        new DocumentJsonSerializerOptionsConfiguration(derivedOptions.Object).Configure(_jsonOptions);
        _store.Configuration.ContentSerializer = new DefaultContentJsonSerializer(Options.Create(_jsonOptions));

        // The real migrations create the tables; their deferred work needs a shell scope, so it doesn't run here.
        await using (var session = _store.CreateSession())
        {
            var migrations = new WorkflowMigrations { SchemaBuilder = new SchemaBuilder(_store.Configuration, await session.BeginTransactionAsync()) };
            await migrations.CreateAsync();
            await migrations.UpdateFrom4Async();
            await migrations.UpdateFrom5Async();
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        _store.RegisterIndexes<WorkflowTypeIndexProvider>();
        _store.RegisterIndexes<WorkflowIndexProvider>();
        _store.RegisterIndexes<WorkflowTypeVersionIndexProvider>();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var session in _sessions)
        {
            await session.DisposeAsync();
        }

        _store?.Dispose();

        // Pooled connections keep the database file open.
        SqliteConnection.ClearAllPools();

        if (File.Exists(_tempFilename))
        {
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

    [Fact]
    public async Task SaveAsync_NewWorkflowType_CreatesVersionOneAndSetsVersionId()
    {
        var (session, _, types) = CreateStores();
        var workflowType = CreateWorkflowType();

        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var version = Assert.Single(await CreateStores().Versions.ListAsync("type-1"));
        Assert.Equal(1, version.Version);
        Assert.Equal(workflowType.VersionId, version.VersionId);
        Assert.Equal(26, version.VersionId.Length);
        Assert.Equal("Approval", version.Name);
        Assert.Equal(s_now, version.CreatedUtc);
        Assert.Equal(new[] { "start", "notify" }, version.Activities.Select(activity => activity.ActivityId));
        Assert.Single(version.Transitions);
    }

    [Fact]
    public async Task SaveAsync_RenamedOrDisabled_CreatesNoVersion()
    {
        var workflowType = await SaveNewAsync();
        var firstVersionId = workflowType.VersionId;

        var (session, _, types) = CreateStores();
        workflowType = await types.GetAsync("type-1");
        workflowType.Name = "Renamed";
        workflowType.IsEnabled = false;
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Single(await CreateStores().Versions.ListAsync("type-1"));
        Assert.Equal(firstVersionId, workflowType.VersionId);
    }

    [Theory]
    [InlineData("activity")]
    [InlineData("position")]
    [InlineData("transition")]
    [InlineData("setting")]
    public async Task SaveAsync_DefinitionChanged_CreatesTheNextVersion(string change)
    {
        var workflowType = await SaveNewAsync();
        var firstVersionId = workflowType.VersionId;

        var (session, _, types) = CreateStores();
        workflowType = await types.GetAsync("type-1");

        switch (change)
        {
            case "activity":
                workflowType.Activities[1].Properties["Message"] = "Changed";
                break;
            case "position":
                workflowType.Activities[1].X += 20;
                break;
            case "transition":
                workflowType.Transitions.Clear();
                break;
            case "setting":
                workflowType.DeleteFinishedWorkflows = true;
                break;
        }

        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var all = (await CreateStores().Versions.ListAsync("type-1")).ToList();
        Assert.Equal(new[] { 2, 1 }, all.Select(version => version.Version));
        Assert.Equal(all[0].VersionId, workflowType.VersionId);
        Assert.NotEqual(firstVersionId, workflowType.VersionId);
        Assert.Equal(all[0].VersionId, (await CreateStores().Versions.GetLatestAsync("type-1")).VersionId);
    }

    [Fact]
    public async Task CreateIfChangedAsync_HeadChangedAfterwards_DoesNotChangeTheVersion()
    {
        var (session, versions, _) = CreateStores();
        var workflowType = CreateWorkflowType();

        var version = await versions.CreateIfChangedAsync(workflowType);
        workflowType.Activities[1].Properties["Message"] = "Changed later";
        workflowType.Transitions[0].SourceOutcomeName = "Other";
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var stored = await CreateStores().Versions.GetAsync(version.VersionId);
        Assert.Equal("Hello", stored.Activities[1].Properties["Message"]!.GetValue<string>());
        Assert.Equal("Done", stored.Transitions[0].SourceOutcomeName);
    }

    [Fact]
    public async Task DeleteAsync_WorkflowType_DeletesItsVersions()
    {
        await SaveNewAsync();

        var (session, _, types) = CreateStores();
        var workflowType = await types.GetAsync("type-1");
        workflowType.Activities[1].X = 500;
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, (await CreateStores().Versions.ListAsync("type-1")).Count());

        (session, _, types) = CreateStores();
        await types.DeleteAsync(await types.GetAsync("type-1"));
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await CreateStores().Versions.ListAsync("type-1"));
    }

    [Fact]
    public async Task CreateInitialVersionsAsync_TypesWithoutVersions_CreatesVersionOneOnce()
    {
        // Workflow types saved before versions existed.
        await using (var session = _store.CreateSession())
        {
            await session.SaveAsync(CreateWorkflowType("type-1"), cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveAsync(CreateWorkflowType("type-2"), cancellationToken: TestContext.Current.CancellationToken);
            await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var (firstSession, firstVersions, _) = CreateStores();
        Assert.Equal(2, await WorkflowMigrations.CreateInitialVersionsAsync(firstSession, firstVersions));
        await firstSession.SaveChangesAsync(TestContext.Current.CancellationToken);

        var (secondSession, secondVersions, types) = CreateStores();
        Assert.Equal(0, await WorkflowMigrations.CreateInitialVersionsAsync(secondSession, secondVersions));

        foreach (var workflowTypeId in new[] { "type-1", "type-2" })
        {
            var version = Assert.Single(await secondVersions.ListAsync(workflowTypeId));
            Assert.Equal(1, version.Version);
            Assert.Equal(version.VersionId, (await types.GetAsync(workflowTypeId)).VersionId);
        }
    }

    [Fact]
    public void ProcessWorkflowType_Export_LeavesTheVersionIdOut()
    {
        var workflowType = CreateWorkflowType();
        workflowType.VersionId = "version-1";
        var result = new DeploymentPlanResult(new MemoryFileBuilder(), new RecipeDescriptor());

        AllWorkflowTypeDeploymentSource.ProcessWorkflowType(result, [workflowType], _jsonOptions.SerializerOptions);

        var exported = Assert.Single(result.Steps.Single()["data"]!.AsArray())!.AsObject();
        Assert.False(exported.ContainsKey(nameof(WorkflowType.VersionId)));
        Assert.False(exported.ContainsKey(nameof(WorkflowType.Id)));
        Assert.Equal("type-1", exported[nameof(WorkflowType.WorkflowTypeId)]!.GetValue<string>());
    }

    private async Task<WorkflowType> SaveNewAsync()
    {
        var (session, _, types) = CreateStores();
        var workflowType = CreateWorkflowType();
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        return workflowType;
    }

    private (ISession Session, WorkflowTypeVersionStore Versions, WorkflowTypeStore Types) CreateStores()
    {
        var session = _store.CreateSession();
        _sessions.Add(session);

        var idGenerator = new Mock<IIdGenerator>();
        idGenerator.Setup(x => x.GenerateUniqueId()).Returns(() => IdGenerator.GenerateId());

        var versions = new WorkflowTypeVersionStore(
            session,
            idGenerator.Object,
            Mock.Of<IClock>(x => x.UtcNow == s_now),
            Mock.Of<IHttpContextAccessor>(),
            Options.Create(_jsonOptions));

        var types = new WorkflowTypeStore(session, versions, [], NullLogger<WorkflowTypeStore>.Instance);

        return (session, versions, types);
    }

    private static WorkflowType CreateWorkflowType(string workflowTypeId = "type-1")
        => new()
        {
            WorkflowTypeId = workflowTypeId,
            Name = "Approval",
            IsEnabled = true,
            Activities =
            [
                new ActivityRecord { ActivityId = "start", Name = "SignalEvent", IsStart = true, X = 10, Y = 10 },
                new ActivityRecord { ActivityId = "notify", Name = "NotifyTask", X = 300, Y = 10, Properties = new JsonObject { ["Message"] = "Hello" } },
            ],
            Transitions =
            [
                new Transition { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "notify" },
            ],
        };
}
