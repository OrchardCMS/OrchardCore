using System.Text.Json.Nodes;
using OrchardCore.Documents;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Designer;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

public sealed class WorkflowVersionRoutesTests : IClassFixture<WorkflowDesignerSiteFixture>
{
    private readonly WorkflowDesignerSiteFixture _fixture;

    public WorkflowVersionRoutesTests(WorkflowDesignerSiteFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task InstanceRoutes_InstancePinnedToAnEarlierVersion_ComeFromThatVersion()
    {
        var context = _fixture.Context;
        var (id, workflowTypeId) = await WorkflowDesignerSiteFixture.CreateWorkflowTypeAsync(context, new ActivityRecord
        {
            ActivityId = "filter",
            Name = "HttpRequestFilterEvent",
            Properties = new JsonObject { ["HttpMethod"] = "GET" },
        });

        // Version 2 waits for POST requests instead.
        string firstVersionId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowTypeStore = scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>();
            var workflowType = await workflowTypeStore.GetAsync(id);
            firstVersionId = workflowType.VersionId;
            workflowType.Activities[0].Properties["HttpMethod"] = "POST";
            await workflowTypeStore.SaveAsync(workflowType);
        });

        var workflowId = Guid.NewGuid().ToString("n");

        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<IWorkflowStore>().SaveAsync(new Workflow
        {
            WorkflowId = workflowId,
            WorkflowTypeId = workflowTypeId,
            WorkflowTypeVersionId = firstVersionId,
            Status = WorkflowStatus.Halted,
            CreatedUtc = DateTime.UtcNow,
            BlockingActivities = [new BlockingActivity { ActivityId = "filter", Name = "HttpRequestFilterEvent" }],
        }));

        IList<WorkflowRoutesEntry> entries = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var document = await scope.ServiceProvider.GetRequiredService<IVolatileDocumentManager<WorkflowRouteDocument>>().GetOrCreateImmutableAsync();
            entries = document.Entries[workflowId];
        });

        Assert.Equal("GET", Assert.Single(entries).HttpMethod);
    }

    [Fact]
    public async Task Invoke_ActivityRemovedSinceTheInstanceStarted_ResumesTheInstance()
    {
        var context = _fixture.Context;
        var (id, workflowTypeId) = await WorkflowDesignerSiteFixture.CreateWorkflowTypeAsync(
            context,
            new ActivityRecord { ActivityId = "start", Name = "SignalEvent", IsStart = true },
            new ActivityRecord
            {
                ActivityId = "request",
                Name = "HttpRequestEvent",
                Properties = new JsonObject { ["HttpMethod"] = "GET", ["ValidateAntiforgeryToken"] = false },
            });

        string firstVersionId = null;

        // Version 2 no longer has the HTTP request event.
        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowTypeStore = scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>();
            var workflowType = await workflowTypeStore.GetAsync(id);
            firstVersionId = workflowType.VersionId;
            workflowType.Activities.RemoveAt(1);
            await workflowTypeStore.SaveAsync(workflowType);
        });

        var workflowId = Guid.NewGuid().ToString("n");

        // An instance started on version 1, waiting on the request event.
        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowType = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync(id);
            var firstVersion = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeVersionStore>().GetWorkflowTypeAsync(workflowType, firstVersionId);
            var workflow = scope.ServiceProvider.GetRequiredService<IWorkflowManager>().NewWorkflow(firstVersion);
            workflow.WorkflowId = workflowId;
            workflow.Status = WorkflowStatus.Halted;
            workflow.BlockingActivities.Add(new BlockingActivity { ActivityId = "request", Name = "HttpRequestEvent" });

            Assert.Equal(firstVersionId, workflow.WorkflowTypeVersionId);
            await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().SaveAsync(workflow);
        });

        using var response = await context.Client.GetAsync(await _fixture.CreateInvokeUrlAsync(workflowTypeId, "request"), TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);

        // IWorkflowStore.GetAsync(string) only finds instances that are waiting on an activity.
        Workflow resumed = null;
        await context.UsingTenantScopeAsync(async scope => resumed = await scope.ServiceProvider.GetRequiredService<YesSql.ISession>()
            .Query<Workflow, WorkflowIndex>(index => index.WorkflowId == workflowId)
            .FirstOrDefaultAsync());

        Assert.Equal(WorkflowStatus.Finished, resumed.Status);
    }
}
