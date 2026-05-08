using System.Text.Json.Nodes;
using OrchardCore.DisplayManagement;
using OrchardCore.Json;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;

namespace OrchardCore.Tests.Workflows;

public class WorkflowManagerTests
{
    [Fact]
    public async Task CanExecuteSimpleWorkflow()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var localizer = new Mock<IStringLocalizer<AddTask>>();

        var stringBuilder = new StringBuilder();
        var output = new StringWriter(stringBuilder);
        var addTask = new AddTask(scriptEvaluator, localizer.Object);
        var writeLineTask = new WriteLineTask(scriptEvaluator, localizer.Object, output);
        var setOutputTask = new SetOutputTask(scriptEvaluator, new Mock<IStringLocalizer<SetOutputTask>>().Object);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Activities =
            [
                new()
                {
                    ActivityId = "1",
                    IsStart = true,
                    Name = addTask.Name,
                    Properties = JObject.FromObject(new
                    {
                        A = new WorkflowExpression<double>("input(\"A\")"),
                        B = new WorkflowExpression<double>("input(\"B\")"),
                    }),
                },
                new() { ActivityId = "2", Name = writeLineTask.Name, Properties = JObject.FromObject(new { Text = new WorkflowExpression<string>("lastResult().toString()") }) },
                new() { ActivityId = "3", Name = setOutputTask.Name, Properties = JObject.FromObject(new { Value = new WorkflowExpression<string>("lastResult()"), OutputName = "Sum" }) }
            ],
            Transitions =
            [
                new() { SourceActivityId = "1", SourceOutcomeName = "Done", DestinationActivityId = "2" },
                new() { SourceActivityId = "2", SourceOutcomeName = "Done", DestinationActivityId = "3" }
            ],
        };

        var workflowManager = CreateWorkflowManager(serviceProvider, [addTask, writeLineTask, setOutputTask], workflowType);
        var a = 10d;
        var b = 22d;
        var expectedSum = a + b;
        var expectedResult = expectedSum.ToString() + System.Environment.NewLine;

        var workflowExecutionContext = await workflowManager.StartWorkflowAsync(workflowType, input: new RouteValueDictionary(new { A = a, B = b }));
        var actualResult = stringBuilder.ToString();

        Assert.Equal(expectedResult, actualResult);
        Assert.True(workflowExecutionContext.Output.ContainsKey("Sum"));
        Assert.Equal(expectedSum, (double)workflowExecutionContext.Output["Sum"]);
    }

    [Fact]
    public async Task WorkflowScriptEvaluatorShouldEvaluateAsyncScopedGlobalMethods()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        var result = await scriptEvaluator.EvaluateAsync(
            new WorkflowExpression<string>("getValueAsync()"),
            workflowContext,
            new AsyncStringMethodProvider());

        Assert.Equal("async", result);
    }

    [Fact]
    public async Task TriggerEventAsync_ShouldAllowReexecutionAfterUnexpectedError()
    {
        const string nonExistentActivityId = "missing";

        var serviceProvider = CreateServiceProvider();
        var executionCount = 0;
        var countingTask = new CountingTask(() => executionCount++);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Activities =
            [
                new()
                {
                    ActivityId = "1",
                    IsStart = true,
                    Name = countingTask.Name,
                },
            ],
            Transitions =
            [
                new() { SourceActivityId = "1", SourceOutcomeName = "Done", DestinationActivityId = nonExistentActivityId },
            ],
        };

        var workflowManager = CreateWorkflowManager(serviceProvider, [countingTask], workflowType);

        await Assert.ThrowsAsync<NullReferenceException>(() => workflowManager.TriggerEventAsync(countingTask.Name));
        await Assert.ThrowsAsync<NullReferenceException>(() => workflowManager.TriggerEventAsync(countingTask.Name));

        Assert.Equal(2, executionCount);
    }

    [Fact]
    public async Task TriggerEventAsync_ShouldAllowFaultHandlerToTriggerWorkflowAfterActivityError()
    {
        var serviceProvider = CreateServiceProvider();
        var executionCount = 0;
        var faultTriggerCount = 0;
        var throwingTask = new ThrowingTask(() => executionCount++);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Activities =
            [
                new()
                {
                    ActivityId = "1",
                    IsStart = true,
                    Name = throwingTask.Name,
                },
            ],
        };

        var workflowManager = CreateWorkflowManager(serviceProvider, [throwingTask], workflowType, (workflowFaultHandler, manager) =>
        {
            workflowFaultHandler
                .Setup(x => x.OnWorkflowFaultAsync(It.IsAny<IWorkflowManager>(), It.IsAny<WorkflowExecutionContext>(), It.IsAny<ActivityContext>(), It.IsAny<Exception>()))
                .Returns(async () =>
                {
                    if (faultTriggerCount++ == 0)
                    {
                        await manager.TriggerEventAsync(throwingTask.Name);
                    }
                });
        });

        await workflowManager.TriggerEventAsync(throwingTask.Name);

        Assert.Equal(2, executionCount);
        Assert.Equal(2, faultTriggerCount);
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped(typeof(Resolver<>));
        services.AddScoped(provider => new Mock<IShapeFactory>().Object);
        services.AddScoped(provider => new Mock<IViewLocalizer>().Object);
        services.AddScoped<IWorkflowExecutionContextHandler, DefaultWorkflowExecutionContextHandler>();

        return services.BuildServiceProvider();
    }

    private static JavaScriptWorkflowScriptEvaluator CreateWorkflowScriptEvaluator(IServiceProvider serviceProvider)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var javaScriptEngine = new JavaScriptEngine(memoryCache, Options.Create(new Jint.Options()));
        var workflowContextHandlers = new Resolver<IEnumerable<IWorkflowExecutionContextHandler>>(serviceProvider);
        var globalMethodProviders = Array.Empty<IGlobalMethodProvider>();
        var scriptingManager = new DefaultScriptingManager(new[] { javaScriptEngine }, globalMethodProviders);

        return new JavaScriptWorkflowScriptEvaluator(
            scriptingManager,
            workflowContextHandlers.Resolve(),
            new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object
        );
    }

    private static WorkflowManager CreateWorkflowManager(
        IServiceProvider serviceProvider,
        IEnumerable<IActivity> activities,
        WorkflowType workflowType,
        Action<Mock<IWorkflowFaultHandler>, WorkflowManager> configureWorkflowFaultHandler = null
    )
    {
        var workflowValueSerializers = new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider);
        var activityLibrary = new Mock<IActivityLibrary>();
        var workflowTypeStore = new Mock<IWorkflowTypeStore>();
        var workflowStore = new Mock<IWorkflowStore>();
        var workflowIdGenerator = new Mock<IWorkflowIdGenerator>();
        workflowIdGenerator.Setup(x => x.GenerateUniqueId(It.IsAny<Workflow>())).Returns(IdGenerator.GenerateId());
        var distributedLock = new Mock<IDistributedLock>();
        var workflowManagerLogger = new Mock<ILogger<WorkflowManager>>();
        var workflowContextLogger = new Mock<ILogger<WorkflowExecutionContext>>();
        var missingActivityLogger = new Mock<ILogger<MissingActivity>>();
        var missingActivityLocalizer = new Mock<IStringLocalizer<MissingActivity>>();
        var clock = new Mock<IClock>();
        var workflowFaultHandler = new Mock<IWorkflowFaultHandler>();
        var jsonOptionsMock = new Mock<IOptions<DocumentJsonSerializerOptions>>();
        jsonOptionsMock.Setup(x => x.Value)
            .Returns(new DocumentJsonSerializerOptions());

        var workflowManager = new WorkflowManager(
            activityLibrary.Object,
            workflowTypeStore.Object,
            workflowStore.Object,
            workflowIdGenerator.Object,
            workflowValueSerializers,
            workflowFaultHandler.Object,
            distributedLock.Object,
            workflowManagerLogger.Object,
            missingActivityLogger.Object,
            missingActivityLocalizer.Object,
            jsonOptionsMock.Object,
            clock.Object
            );

        configureWorkflowFaultHandler?.Invoke(workflowFaultHandler, workflowManager);

        foreach (var activity in activities)
        {
            activityLibrary.Setup(x => x.InstantiateActivity(activity.Name)).Returns(activity);
            activityLibrary.Setup(x => x.GetActivityByName(activity.Name)).Returns(activity);
        }

        workflowTypeStore.Setup(x => x.GetAsync(workflowType.Id)).Returns(Task.FromResult(workflowType));
        workflowTypeStore.Setup(x => x.GetByStartActivityAsync(It.IsAny<string>()))
            .ReturnsAsync((string activityName) => workflowType.Activities.Any(x => x.IsStart && x.Name == activityName) ? [workflowType] : []);
        workflowStore.Setup(x => x.ListByActivityNameAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([]);
        workflowStore.Setup(x => x.HasHaltedInstanceAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        workflowStore.Setup(x => x.ListAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync([]);

        return workflowManager;
    }

    private sealed class CountingTask : TaskActivity<CountingTask>
    {
        private readonly Action _onExecute;

        public CountingTask(Action onExecute)
        {
            _onExecute = onExecute;
        }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(new LocalizedString("Done", "Done"));
        }

        public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _onExecute();

            return Task.FromResult(Outcomes("Done"));
        }
    }

    private sealed class ThrowingTask : TaskActivity<ThrowingTask>
    {
        private readonly Action _onExecute;

        public ThrowingTask(Action onExecute)
        {
            _onExecute = onExecute;
        }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _onExecute();

            throw new InvalidOperationException("Simulated activity failure");
        }
    }

    private sealed class AsyncStringMethodProvider : IGlobalMethodProvider
    {
        public IEnumerable<GlobalMethod> GetMethods()
        {
            yield return new GlobalMethod
            {
                Name = "getValueAsync",
                Method = serviceProvider => (Func<string>)(() => "sync"),
                AsyncMethod = serviceProvider => (Func<Task<string>>)(async () =>
                {
                    await Task.Yield();
                    return "async";
                }),
            };
        }
    }
}
