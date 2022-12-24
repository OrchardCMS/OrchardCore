using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Testing.Mocks;

public static partial class OrchardCoreMock
{
    public static WorkflowManager CreateWorkflowManager(
        IServiceProvider serviceProvider,
        IEnumerable<IActivity> activities,
        WorkflowType workflowType)
    {
        var workflowValueSerializers = new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider);
        var activityLibrary = new Mock<IActivityLibrary>();
        var workflowTypeStore = new Mock<IWorkflowTypeStore>();
        var workflowStore = new Mock<IWorkflowStore>();
        var workflowIdGenerator = new Mock<IWorkflowIdGenerator>();
        workflowIdGenerator
            .Setup(x => x.GenerateUniqueId(It.IsAny<Workflow>()))
            .Returns(IdGenerator.GenerateId());

        var distributedLock = new Mock<IDistributedLock>();
        var workflowManagerLogger = new Mock<ILogger<WorkflowManager>>();
        var workflowContextLogger = new Mock<ILogger<WorkflowExecutionContext>>();
        var missingActivityLogger = new Mock<ILogger<MissingActivity>>();
        var missingActivityLocalizer = new Mock<IStringLocalizer<MissingActivity>>();
        var clock = new Mock<IClock>();
        var workflowManager = new WorkflowManager(
            activityLibrary.Object,
            workflowTypeStore.Object,
            workflowStore.Object,
            workflowIdGenerator.Object,
            workflowValueSerializers,
            distributedLock.Object,
            workflowManagerLogger.Object,
            missingActivityLogger.Object,
            missingActivityLocalizer.Object,
            clock.Object);

        foreach (var activity in activities)
        {
            activityLibrary.Setup(x => x.InstantiateActivity(activity.Name)).Returns(activity);
        }

        workflowTypeStore.Setup(x => x.GetAsync(workflowType.Id)).Returns(Task.FromResult(workflowType));

        return workflowManager;
    }

    public static IWorkflowScriptEvaluator CreateWorkflowScriptEvaluator(IServiceProvider serviceProvider)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var javaScriptEngine = new JavaScriptEngine(memoryCache);
        var workflowContextHandlers = new Resolver<IEnumerable<IWorkflowExecutionContextHandler>>(serviceProvider);
        var globalMethodProviders = new IGlobalMethodProvider[0];
        var scriptingManager = new DefaultScriptingManager(new[] { javaScriptEngine }, globalMethodProviders);

        return new JavaScriptWorkflowScriptEvaluator(
            scriptingManager,
            workflowContextHandlers.Resolve(),
            new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object
        );
    }
}
