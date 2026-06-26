using System.Collections;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Json;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Liquid;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Expressions;
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
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>().Object;
        var setOutputTask = new SetOutputTask(scriptEvaluator, expressionEvaluator, new Mock<IStringLocalizer<SetOutputTask>>().Object);
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
    public async Task SetOutputTaskShouldEvaluateLiquidExpression()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        workflowContext.Properties["Greeting"] = "Hello";
        expressionEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), workflowContext, null))
            .ReturnsAsync("Hello world");

        var activity = new SetOutputTask(scriptEvaluator, expressionEvaluator.Object, new Mock<IStringLocalizer<SetOutputTask>>().Object)
        {
            OutputName = "Message",
            Syntax = WorkflowScriptSyntax.Liquid,
            LiquidValue = new WorkflowExpression<object>("{{ Workflow.Properties.Greeting }} world"),
        };

        await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Equal("Hello world", workflowContext.Output["Message"]);
        expressionEvaluator.Verify(x => x.EvaluateAsync(
            It.Is<WorkflowExpression<object>>(expression => expression.Expression == "{{ Workflow.Properties.Greeting }} world"),
            workflowContext,
            null), Times.Once);
    }

    [Fact]
    public async Task SetPropertyTaskShouldEvaluateLiquidExpression()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        workflowContext.Output["Value"] = "Saved";
        expressionEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), workflowContext, null))
            .ReturnsAsync("Saved value");

        var activity = new SetPropertyTask(scriptEvaluator, expressionEvaluator.Object, new Mock<IStringLocalizer<SetPropertyTask>>().Object)
        {
            PropertyName = "Message",
            Syntax = WorkflowScriptSyntax.Liquid,
            LiquidValue = new WorkflowExpression<object>("{{ Workflow.Output.Value }} value"),
        };

        await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Equal("Saved value", workflowContext.Properties["Message"]);
        expressionEvaluator.Verify(x => x.EvaluateAsync(
            It.Is<WorkflowExpression<object>>(expression => expression.Expression == "{{ Workflow.Output.Value }} value"),
            workflowContext,
            null), Times.Once);
    }

    [Fact]
    public async Task IfElseTaskShouldEvaluateLiquidExpression()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        expressionEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<bool>>(), workflowContext, null))
            .ReturnsAsync(true);

        var activity = new IfElseTask(scriptEvaluator, expressionEvaluator.Object, new Mock<IStringLocalizer<IfElseTask>>().Object)
        {
            Syntax = WorkflowScriptSyntax.Liquid,
            LiquidCondition = new WorkflowExpression<bool>("{{ Workflow.Properties.ShouldRun }}"),
        };

        var result = await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Contains("True", result.Outcomes);
        expressionEvaluator.Verify(x => x.EvaluateAsync(
            It.Is<WorkflowExpression<bool>>(expression => expression.Expression == "{{ Workflow.Properties.ShouldRun }}"),
            workflowContext,
            null), Times.Once);
    }

    [Fact]
    public async Task WhileLoopTaskShouldEvaluateLiquidExpression()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        expressionEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<bool>>(), workflowContext, null))
            .ReturnsAsync(true);

        var activity = new WhileLoopTask(scriptEvaluator, expressionEvaluator.Object, new Mock<IStringLocalizer<WhileLoopTask>>().Object)
        {
            Syntax = WorkflowScriptSyntax.Liquid,
            LiquidCondition = new WorkflowExpression<bool>("{{ Workflow.Properties.ShouldLoop }}"),
        };

        var result = await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Contains("Iterate", result.Outcomes);
        expressionEvaluator.Verify(x => x.EvaluateAsync(
            It.Is<WorkflowExpression<bool>>(expression => expression.Expression == "{{ Workflow.Properties.ShouldLoop }}"),
            workflowContext,
            null), Times.Once);
    }

    [Fact]
    public async Task ForLoopTaskShouldEvaluateLiquidExpressions()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        expressionEvaluator
            .SetupSequence(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<string>>(), workflowContext, null))
            .ReturnsAsync("1")
            .ReturnsAsync("3")
            .ReturnsAsync("1");

        var activity = new ForLoopTask(scriptEvaluator, expressionEvaluator.Object, new Mock<IStringLocalizer<ForLoopTask>>().Object)
        {
            Syntax = WorkflowScriptSyntax.Liquid,
            LiquidFrom = new WorkflowExpression<string>("{{ Workflow.Properties.From }}"),
            LiquidTo = new WorkflowExpression<string>("{{ Workflow.Properties.To }}"),
            LiquidStep = new WorkflowExpression<string>("{{ Workflow.Properties.Step }}"),
        };

        var result = await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Contains("Iterate", result.Outcomes);
        Assert.Equal(1d, workflowContext.LastResult);
        Assert.Equal(1d, workflowContext.Properties["x"]);
    }

    [Fact]
    public async Task ForEachTaskShouldEvaluateLiquidExpression()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        expressionEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), workflowContext, null))
            .ReturnsAsync(new[] { "a", "b" });

        var activity = new ForEachTask(scriptEvaluator, expressionEvaluator.Object, new Mock<IStringLocalizer<ForEachTask>>().Object)
        {
            Syntax = WorkflowScriptSyntax.Liquid,
            LiquidEnumerable = new WorkflowExpression<object>("{{ Workflow.Properties.Items | json }}"),
        };

        var result = await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Contains("Iterate", result.Outcomes);
        Assert.Equal("a", activity.Current?.ToString());
        Assert.Equal("a", workflowContext.LastResult?.ToString());
        Assert.Equal("a", workflowContext.Properties["x"]?.ToString());
    }

    [Fact]
    public async Task LiquidTaskShouldSetLastResult()
    {
        var serviceProvider = CreateServiceProvider();
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>();
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        expressionEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), workflowContext, null))
            .ReturnsAsync("Hello");

        var activity = new LiquidTask(expressionEvaluator.Object, new Mock<IStringLocalizer<LiquidTask>>().Object)
        {
            Expression = new WorkflowExpression<object>("{{ Workflow.Properties.Greeting }}"),
        };

        var result = await activity.ExecuteAsync(workflowContext, new ActivityContext());

        Assert.Contains("Done", result.Outcomes);
        Assert.Equal("Hello", workflowContext.LastResult);
    }

    [Fact]
    public async Task LiquidWorkflowExpressionEvaluatorShouldPreserveCollectionValues()
    {
        using var serviceProvider = CreateLiquidWorkflowServiceProvider();
        var evaluator = CreateLiquidWorkflowExpressionEvaluator(serviceProvider);
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        var result = await evaluator.EvaluateAsync(
            new WorkflowExpression<object>("{{ 'a,b' | split: ',' }}"),
            workflowContext,
            null);

        var items = Assert.IsAssignableFrom<IEnumerable>(result).Cast<object>().Select(x => x?.ToString()).ToArray();
        Assert.Equal(["a", "b"], items);
    }

    [Fact]
    public async Task LiquidWorkflowExpressionEvaluatorShouldEvaluateBooleanComparison()
    {
        using var serviceProvider = CreateLiquidWorkflowServiceProvider();
        var evaluator = CreateLiquidWorkflowExpressionEvaluator(serviceProvider);
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        workflowContext.Properties["Items"] = new[] { "a", "b" };

        var result = await evaluator.EvaluateAsync(
            new WorkflowExpression<bool>("{{ Workflow.Properties[\"Items\"].size > 0 }}"),
            workflowContext,
            null);

        Assert.True(result);
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

    private static ServiceProvider CreateLiquidWorkflowServiceProvider()
    {
        var services = new ServiceCollection();
        services.Configure<FluidParserOptions>(_ => { });
        services.Configure<LiquidViewOptions>(_ => { });
        services.Configure<TemplateOptions>(options =>
        {
            options.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));
            options.MemberAccessStrategy.Register<WorkflowExecutionContext>();
            options.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Input", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Input, name, context)));
            options.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Output", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Output, name, context)));
            options.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Properties", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Properties, name, context)));
        });

        return services.BuildServiceProvider();
    }

    private static LiquidWorkflowExpressionEvaluator CreateLiquidWorkflowExpressionEvaluator(ServiceProvider serviceProvider)
        => new(
            new LiquidViewParser(
                serviceProvider.GetRequiredService<IOptions<LiquidViewOptions>>(),
                serviceProvider.GetRequiredService<IOptions<FluidParserOptions>>()),
            [],
            serviceProvider,
            new Mock<ILogger<LiquidWorkflowExpressionEvaluator>>().Object,
            serviceProvider.GetRequiredService<IOptions<TemplateOptions>>());

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
                Name = "getValue",
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
