using System.Text.Json.Nodes;
using OrchardCore.Deployment;
using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Stubs;
using OrchardCore.Workflows.Deployment;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using WorkflowMigrations = OrchardCore.Workflows.Migrations;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

public sealed class WorkflowTypeVersionStoreTests : IAsyncLifetime
{
    private VersioningTestDatabase _database;

    public async ValueTask InitializeAsync()
        => _database = await VersioningTestDatabase.CreateAsync();

    public ValueTask DisposeAsync()
        => _database?.DisposeAsync() ?? ValueTask.CompletedTask;

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
        Assert.Equal(VersioningTestDatabase.Now, version.CreatedUtc);
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
    [InlineData("usable as an activity")]
    [InlineData("branching mode")]
    [InlineData("variable")]
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
            case "usable as an activity":
                workflowType.IsActivity = true;
                break;
            case "branching mode":
                workflowType.BranchingMode = WorkflowBranchingMode.All;
                break;
            case "variable":
                workflowType.Variables.Add(new WorkflowVariableDefinition { Name = "total", TypeName = "number", DefaultValue = 0 });
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
        await using (var session = _database.Store.CreateSession())
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
    public async Task GetWorkflowTypeAsync_EarlierVersion_RunsThatVersionWithTheCurrentIdentity()
    {
        var workflowType = await SaveNewAsync();
        var firstVersionId = workflowType.VersionId;

        var (session, _, types) = CreateStores();
        workflowType = await types.GetAsync("type-1");
        workflowType.Name = "Renamed";
        workflowType.Activities[1].Properties["Message"] = "Version 2";
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var versions = CreateStores().Versions;
        var definition = await versions.GetWorkflowTypeAsync(workflowType, firstVersionId);

        Assert.NotSame(workflowType, definition);
        Assert.Equal(workflowType.Id, definition.Id);
        Assert.Equal("Renamed", definition.Name);
        Assert.Equal(firstVersionId, definition.VersionId);
        Assert.Equal("Hello", definition.Activities[1].Properties["Message"]!.GetValue<string>());
    }

    [Fact]
    public async Task GetWorkflowTypeAsync_EarlierVersion_HasItsVariables()
    {
        var workflowType = CreateWorkflowType();
        workflowType.Variables.Add(new WorkflowVariableDefinition { Name = "greeting", TypeName = "string", DefaultValue = "Hello" });
        var (session, _, types) = CreateStores();
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);
        var firstVersionId = workflowType.VersionId;

        (session, _, types) = CreateStores();
        workflowType = await types.GetAsync("type-1");
        workflowType.Variables.Clear();
        await types.SaveAsync(workflowType);
        await session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var definition = await CreateStores().Versions.GetWorkflowTypeAsync(workflowType, firstVersionId);

        var variable = Assert.Single(definition.Variables);
        Assert.Equal("greeting", variable.Name);
        Assert.Equal("Hello", variable.DefaultValue!.GetValue<string>());
        Assert.Empty(workflowType.Variables);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("current")]
    [InlineData("missing")]
    public async Task GetWorkflowTypeAsync_NoEarlierVersion_ReturnsTheWorkflowTypeItself(string versionId)
    {
        var workflowType = await SaveNewAsync();

        var definition = await CreateStores().Versions.GetWorkflowTypeAsync(workflowType, versionId == "current" ? workflowType.VersionId : versionId);

        Assert.Same(workflowType, definition);
    }

    [Fact]
    public void ProcessWorkflowType_Export_LeavesTheVersionIdOut()
    {
        var workflowType = CreateWorkflowType();
        workflowType.VersionId = "version-1";
        var result = new DeploymentPlanResult(new MemoryFileBuilder(), new RecipeDescriptor());

        AllWorkflowTypeDeploymentSource.ProcessWorkflowType(result, [workflowType], _database.JsonOptions.SerializerOptions);

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

    private (YesSql.ISession Session, WorkflowTypeVersionStore Versions, WorkflowTypeStore Types) CreateStores()
        => _database.CreateStores();

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
