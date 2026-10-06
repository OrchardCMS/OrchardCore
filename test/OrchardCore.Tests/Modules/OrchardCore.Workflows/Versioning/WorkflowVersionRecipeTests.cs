using System.Text.Json.Nodes;
using OrchardCore.Environment.Shell;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Workflows.Indexes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Versioning;

public sealed class WorkflowVersionRecipeTests
{
    [Fact]
    public async Task WorkflowTypeRecipeStep_ExistingTypeWithInstances_ImportsTheNextVersionAndKeepsTheInstances()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();
        await EnableFeaturesAsync(context, "OrchardCore.Workflows", "OrchardCore.Workflows.Http", "OrchardCore.Deployment");

        var workflowTypeId = Guid.NewGuid().ToString("n");
        long documentId = 0;
        string firstVersionId = null;
        string workflowId = null;

        // Version 1 waits for the "approve" signal, and an instance is waiting on it.
        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowType = new WorkflowType
            {
                WorkflowTypeId = workflowTypeId,
                Name = "Approval",
                IsEnabled = true,
                Activities =
                [
                    Signal("start", "start", isStart: true),
                    Signal("wait", "approve"),
                ],
                Transitions = [new Transition { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "wait" }],
            };

            await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().SaveAsync(workflowType);
            documentId = workflowType.Id;
            firstVersionId = workflowType.VersionId;

            var workflow = scope.ServiceProvider.GetRequiredService<IWorkflowManager>().NewWorkflow(workflowType);
            workflow.Status = WorkflowStatus.Halted;
            workflow.BlockingActivities.Add(new BlockingActivity { ActivityId = "wait", Name = "SignalEvent" });
            await scope.ServiceProvider.GetRequiredService<IWorkflowStore>().SaveAsync(workflow);
            workflowId = workflow.WorkflowId;
        });

        // The imported definition no longer has the activity the instance waits on.
        await RecipeHelpers.RunRecipeAsync(context, new JsonObject
        {
            ["steps"] = new JsonArray(new JsonObject
            {
                ["name"] = "WorkflowType",
                ["data"] = new JsonArray(new JsonObject
                {
                    ["WorkflowTypeId"] = workflowTypeId,
                    ["Name"] = "Imported",
                    ["IsEnabled"] = true,
                    ["Activities"] = new JsonArray(new JsonObject
                    {
                        ["ActivityId"] = "start",
                        ["Name"] = "SignalEvent",
                        ["IsStart"] = true,
                        ["Properties"] = new JsonObject { ["SignalName"] = new JsonObject { ["Expression"] = "start" } },
                    }),
                    ["Transitions"] = new JsonArray(),
                }),
            }),
        });

        await context.UsingTenantScopeAsync(async scope =>
        {
            var workflowType = await scope.ServiceProvider.GetRequiredService<IWorkflowTypeStore>().GetAsync(workflowTypeId);
            var versions = (await scope.ServiceProvider.GetRequiredService<IWorkflowTypeVersionStore>().ListAsync(workflowTypeId)).ToList();

            Assert.Equal(documentId, workflowType.Id);
            Assert.Equal("Imported", workflowType.Name);
            Assert.Equal(new[] { 2, 1 }, versions.Select(version => version.Version));
            Assert.Equal(versions[0].VersionId, workflowType.VersionId);

            // The instance is still there, pinned to version 1, and resumes on it.
            var workflow = await scope.ServiceProvider.GetRequiredService<ISession>()
                .Query<Workflow, WorkflowIndex>(index => index.WorkflowId == workflowId)
                .FirstOrDefaultAsync();

            Assert.Equal(firstVersionId, workflow.WorkflowTypeVersionId);

            var resumed = await scope.ServiceProvider.GetRequiredService<IWorkflowManager>()
                .ResumeWorkflowAsync(workflow, workflow.BlockingActivities.Single(), new Dictionary<string, object> { ["Signal"] = "approve" });

            Assert.Equal(WorkflowStatus.Finished, resumed.Status);
        });
    }

    private static ActivityRecord Signal(string activityId, string signalName, bool isStart = false)
        => new()
        {
            ActivityId = activityId,
            Name = "SignalEvent",
            IsStart = isStart,
            Properties = new JsonObject { ["SignalName"] = new JsonObject { ["Expression"] = signalName } },
        };

    private static async Task EnableFeaturesAsync(SiteContext context, params string[] featureIds)
    {
        await context.UsingTenantScopeAsync(async scope =>
        {
            var featuresManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var availableFeatures = await featuresManager.GetAvailableFeaturesAsync();
            var features = availableFeatures.Where(feature => featureIds.Contains(feature.Id)).ToArray();
            Assert.Equal(featureIds.Length, features.Length);
            await featuresManager.EnableFeaturesAsync(features, force: true);
        });

        await context.WaitForDeferredTasksAsync(TestContext.Current.CancellationToken);
    }
}
