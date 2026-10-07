using System.Collections;
using System.Text.Json.Nodes;
using Fluid;
using Fluid.Values;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Json;
using OrchardCore.Liquid;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Tests.Workflows.Activities;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.WorkflowContextProviders;

namespace OrchardCore.Tests.Workflows;

public class WorkflowManagerTests
{
    [Fact]
    public async Task CanExecuteSimpleWorkflow_Default_Succeeds()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var localizer = new Mock<IStringLocalizer<AddTask>>();

        var stringBuilder = new StringBuilder();
        var output = new StringWriter(stringBuilder);
        var addTask = new AddTask(scriptEvaluator, localizer.Object);
        var writeLineTask = new WriteLineTask(scriptEvaluator, localizer.Object, output);
        var expressionEvaluator = new Mock<IWorkflowExpressionEvaluator>().Object;
        var setOutputTask = new SetOutputTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator), new Mock<IStringLocalizer<SetOutputTask>>().Object);
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
    public async Task SetOutputTask_Default_EvaluateLiquidExpression()
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

        var activity = new SetOutputTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator.Object), new Mock<IStringLocalizer<SetOutputTask>>().Object)
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
    public async Task SetPropertyTask_Default_EvaluateLiquidExpression()
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

        var activity = new SetPropertyTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator.Object), new Mock<IStringLocalizer<SetPropertyTask>>().Object)
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
    public async Task Default_ElseTaskDefaultEvaluateLiquidExpression_Succeeds()
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

        var activity = new IfElseTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator.Object), new Mock<IStringLocalizer<IfElseTask>>().Object)
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
    public async Task WhileLoopTask_Default_EvaluateLiquidExpression()
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

        var activity = new WhileLoopTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator.Object), new Mock<IStringLocalizer<WhileLoopTask>>().Object)
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
    public async Task ForLoopTask_Default_EvaluateLiquidExpressions()
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

        var activity = new ForLoopTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator.Object), new Mock<IStringLocalizer<ForLoopTask>>().Object)
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
    public async Task ForEachTask_Default_EvaluateLiquidExpression()
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

        var activity = new ForEachTask(TestExpressions.CreateManager(scriptEvaluator, expressionEvaluator.Object), new Mock<IStringLocalizer<ForEachTask>>().Object)
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
    public async Task LiquidTask_Default_SetsLastResult()
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
    public async Task LiquidWorkflowExpressionEvaluator_Default_PreservesCollectionValues()
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
    public async Task LiquidWorkflowExpressionEvaluator_Default_EvaluateBooleanComparison()
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

    [Theory]
    [InlineData("variable(\"count\") + 1", 42d)]
    [InlineData("setVariable(\"count\", \"10\"); variable(\"count\") * 2", 20d)]
    [InlineData("setVariable(\"other\", \"x\"); property(\"other\") + property(\"count\")", "x41")]
    public async Task StartWorkflowAsync_DeclaredVariables_StartWithTheirDefaultsAndScriptsUseThem(string script, object expected)
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var setOutputTask = new SetOutputTask(TestExpressions.CreateManager(scriptEvaluator, new Mock<IWorkflowExpressionEvaluator>().Object), new Mock<IStringLocalizer<SetOutputTask>>().Object);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Variables = [new WorkflowVariableDefinition { Name = "count", TypeName = "number", DefaultValue = 41 }],
            Activities =
            [
                new()
                {
                    ActivityId = "1",
                    IsStart = true,
                    Name = setOutputTask.Name,
                    Properties = JObject.FromObject(new { Value = new WorkflowExpression<object>(script), OutputName = "Result" }),
                },
            ],
        };

        var workflowManager = CreateWorkflowManager(serviceProvider, [setOutputTask], workflowType);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.Equal(expected, workflowContext.Output["Result"]);
    }

    [Fact]
    public async Task StartWorkflowAsync_ScriptSetsAValueOfTheWrongType_LeavesTheVariableUnchanged()
    {
        var serviceProvider = CreateServiceProvider();
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider);
        var setOutputTask = new SetOutputTask(TestExpressions.CreateManager(scriptEvaluator, new Mock<IWorkflowExpressionEvaluator>().Object), new Mock<IStringLocalizer<SetOutputTask>>().Object);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Variables = [new WorkflowVariableDefinition { Name = "count", TypeName = "number" }],
            Activities =
            [
                new()
                {
                    ActivityId = "1",
                    IsStart = true,
                    Name = setOutputTask.Name,
                    Properties = JObject.FromObject(new { Value = new WorkflowExpression<object>("setVariable(\"count\", \"many\")"), OutputName = "Result" }),
                },
            ],
        };

        var workflowManager = CreateWorkflowManager(serviceProvider, [setOutputTask], workflowType);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        // Like any script error, it is logged and the script returns nothing.
        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.False(workflowContext.Properties.ContainsKey("count"));
        Assert.Null(workflowContext.Output["Result"]);
    }

    [Fact]
    public async Task LiquidWorkflowExpressionEvaluator_DeclaredVariable_ReadsItsTypedValue()
    {
        using var serviceProvider = CreateLiquidWorkflowServiceProvider();
        var evaluator = CreateLiquidWorkflowExpressionEvaluator(serviceProvider);
        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType
            {
                Variables =
                [
                    new WorkflowVariableDefinition { Name = "greeting", TypeName = "string", DefaultValue = "Hello" },
                    new WorkflowVariableDefinition { Name = "count", TypeName = "number" },
                ],
            },
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            [],
            TestVariableTypes.CreateProvider());

        workflowContext.Variables.ApplyDefaults();

        // Stored as text through the properties, read as a number through the variables.
        workflowContext.Properties["count"] = "41";

        var result = await evaluator.EvaluateAsync(
            new WorkflowExpression<string>("{{ Workflow.Variables.greeting }} {{ Workflow.Variables.count | plus: 1 }} {{ Workflow.Variables.missing }}"),
            workflowContext,
            null);

        Assert.Equal("Hello 42 ", result);
    }

    [Fact]
    public async Task WorkflowScriptEvaluator_Default_EvaluateAsyncScopedGlobalMethods()
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
    public async Task TriggerEventAsync_Default_AllowsReexecutionAfterUnexpectedError()
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
    public async Task TriggerEventAsync_Default_AllowsFaultHandlerToTriggerWorkflowAfterActivityError()
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

    [Fact]
    public async Task StartWorkflowAsync_ScriptStoppedByAnExecutionLimit_FaultsTheWorkflow()
    {
        var serviceProvider = CreateServiceProvider();
        var jintOptions = new Jint.Options();
        Jint.ConstraintsOptionsExtensions.MaxStatements(jintOptions, 1_000);
        var scriptEvaluator = CreateWorkflowScriptEvaluator(serviceProvider, jintOptions);
        var localizer = new Mock<IStringLocalizer<WriteLineTask>>();

        var stringBuilder = new StringBuilder();
        var output = new StringWriter(stringBuilder);
        var ifElseTask = new IfElseTask(TestExpressions.CreateManager(scriptEvaluator), new Mock<IStringLocalizer<IfElseTask>>().Object);
        var writeLineTask = new WriteLineTask(scriptEvaluator, localizer.Object, output);
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
                    Name = ifElseTask.Name,
                    Properties = JObject.FromObject(new { Condition = new WorkflowExpression<bool>("while (true) {} true") }),
                },
                new() { ActivityId = "2", Name = writeLineTask.Name, Properties = JObject.FromObject(new { Text = new WorkflowExpression<string>("'took the false branch'") }) },
            ],
            Transitions =
            [
                new() { SourceActivityId = "1", SourceOutcomeName = "False", DestinationActivityId = "2" },
            ],
        };

        var workflowManager = CreateWorkflowManager(serviceProvider, [ifElseTask, writeLineTask], workflowType);

        var workflowExecutionContext = await workflowManager.StartWorkflowAsync(workflowType);

        // The condition never finished, so neither outcome was decided: the workflow has to stop as faulted
        // rather than take the False branch as though the script had answered false.
        Assert.Equal(WorkflowStatus.Faulted, workflowExecutionContext.Status);
        Assert.Empty(stringBuilder.ToString());
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
            options.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Variables", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Variables, name, context)));
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

    private static JavaScriptWorkflowScriptEvaluator CreateWorkflowScriptEvaluator(IServiceProvider serviceProvider, Jint.Options jintOptions = null)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var globalMethodProviders = Array.Empty<IGlobalMethodProvider>();
        var javaScriptEngine = new JavaScriptEngine(memoryCache, Options.Create(jintOptions ?? new Jint.Options()), globalMethodProviders);
        var workflowContextHandlers = new Resolver<IEnumerable<IWorkflowExecutionContextHandler>>(serviceProvider);
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
        Action<Mock<IWorkflowFaultHandler>, WorkflowManager> configureWorkflowFaultHandler = null,
        IWorkflowExecutionJournal journal = null,
        IWorkflowDesignerNotifier notifier = null
    )
    {
        var workflowValueSerializers = new Resolver<IEnumerable<IWorkflowValueSerializer>>(serviceProvider);
        var activityLibrary = new Mock<IActivityLibrary>();
        var workflowTypeStore = new Mock<IWorkflowTypeStore>();

        // Instances run the definition they are given: these tests don't publish versions.
        var workflowTypeVersionStore = new Mock<IWorkflowTypeVersionStore>();
        workflowTypeVersionStore.Setup(x => x.GetWorkflowTypeAsync(It.IsAny<WorkflowType>(), It.IsAny<string>()))
            .ReturnsAsync((WorkflowType type, string _) => type);
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
            workflowTypeVersionStore.Object,
            TestVariableTypes.CreateProvider(),
            workflowStore.Object,
            journal ?? Mock.Of<IWorkflowExecutionJournal>(),
            notifier ?? Mock.Of<IWorkflowDesignerNotifier>(),
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
        workflowTypeStore.Setup(x => x.GetAsync(workflowType.WorkflowTypeId)).Returns(Task.FromResult(workflowType));
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

    [Fact]
    public async Task StartWorkflowAsync_BoundOutput_IsWrittenToItsVariable()
    {
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: new() { ["Value"] = "answer" });

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.Equal(42d, workflowContext.Properties["answer"]);
    }

    [Fact]
    public async Task StartWorkflowAsync_Input_SetsTheInputVariablesOfTheSameName()
    {
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: []);
        workflowType.Variables.Add(new WorkflowVariableDefinition { Name = "amount", TypeName = "number", IsInput = true, DefaultValue = 1 });
        workflowType.Variables.Add(new WorkflowVariableDefinition { Name = "count", TypeName = "number", IsInput = true, DefaultValue = 1 });

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType, input: new Dictionary<string, object>
        {
            ["amount"] = "7",
            ["count"] = "many",
            ["answer"] = "3",
        });

        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.Equal(7d, workflowContext.Variables["amount"]);

        // A value that doesn't convert leaves the default, and a variable that isn't an input isn't set.
        Assert.Equal(1d, workflowContext.Variables["count"]);
        Assert.False(workflowContext.Properties.ContainsKey("answer"));
    }

    [Fact]
    public async Task StartWorkflowAsync_OutputWithoutBinding_ChangesNoVariable()
    {
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: []);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.False(workflowContext.Properties.ContainsKey("answer"));
    }

    [Fact]
    public async Task StartWorkflowAsync_BoundOutputOfTheWrongType_FaultsTheWorkflow()
    {
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "many", bindings: new() { ["Value"] = "answer" });

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Faulted, workflowContext.Status);
        Assert.Contains("'answer'", workflowContext.Workflow.FaultMessage);
    }

    [Fact]
    public async Task StartWorkflowAsync_HaltedActivity_WritesNoBoundOutput()
    {
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: new() { ["Value"] = "answer" }, halt: true);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Halted, workflowContext.Status);
        Assert.False(workflowContext.Properties.ContainsKey("answer"));
    }

    [Fact]
    public async Task StartWorkflowAsync_Journal_RecordsEachActivityWithItsOutcomes()
    {
        var (journal, saved) = CreateJournal();
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: [], journal: journal);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.Equal([1, 2], saved.Select(record => record.Sequence));
        Assert.Equal(["start", "output"], saved.Select(record => record.ActivityId));
        Assert.All(saved, record => Assert.Equal(WorkflowExecutionRecordStatus.Completed, record.Status));
        Assert.All(saved, record => Assert.Equal(["Done"], record.Outcomes));
        Assert.Equal("OutputTask", saved[1].ActivityName);
        Assert.All(saved, record => Assert.True(record.DurationMilliseconds >= 0));
        Assert.Equal(workflowContext.Workflow.WorkflowId, saved[0].WorkflowId);

        // The state keeps the executed activities, oldest first, and the last sequence number.
        var state = workflowContext.Workflow.State.ToObject<WorkflowState>();
        Assert.Equal(["start", "output"], state.ExecutedActivities.Select(executed => executed.ActivityId));
        Assert.Equal(2, state.ExecutionSequence);
    }

    [Fact]
    public async Task ResumeWorkflowAsync_HaltedInstance_ContinuesTheJournalSequence()
    {
        var (journal, saved) = CreateJournal();
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: [], halt: true, journal: journal);

        var started = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Halted, started.Status);
        Assert.Equal([WorkflowExecutionRecordStatus.Completed, WorkflowExecutionRecordStatus.Halted], saved.Select(record => record.Status));
        Assert.Empty(saved[1].Outcomes);

        var workflow = started.Workflow;
        var resumed = await workflowManager.ResumeWorkflowAsync(workflow, workflow.BlockingActivities.Single());

        var record = saved.Last();
        Assert.Equal(WorkflowStatus.Halted, resumed.Status);
        Assert.Equal(3, record.Sequence);
        Assert.True(record.IsResume);
        Assert.Equal("output", record.ActivityId);
    }

    [Fact]
    public async Task StartWorkflowAsync_FaultingActivity_RecordsTheError()
    {
        var (journal, saved) = CreateJournal();
        var throwingTask = new ThrowingTask(() => { });
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Activities = [new() { ActivityId = "throw", IsStart = true, Name = throwingTask.Name }],
            Transitions = [],
        };
        var workflowManager = CreateWorkflowManager(CreateServiceProvider(), [throwingTask], workflowType, journal: journal);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        var record = Assert.Single(saved);
        Assert.Equal(WorkflowStatus.Faulted, workflowContext.Status);
        Assert.Equal(WorkflowExecutionRecordStatus.Faulted, record.Status);
        Assert.Equal("Simulated activity failure", record.Error);
    }

    [Fact]
    public async Task StartWorkflowAsync_JournalDisabled_SavesNoRecordButKeepsTheExecutedActivities()
    {
        var (journal, saved) = CreateJournal(enabled: false);
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: [], journal: journal);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Empty(saved);
        Assert.Equal(2, workflowContext.Workflow.State.ToObject<WorkflowState>().ExecutedActivities.Count);
    }

    [Fact]
    public void RecordExecution_ManyActivities_KeepsTheMostRecentExecutedActivities()
    {
        var activity = new ActivityContext { ActivityRecord = new ActivityRecord { ActivityId = "loop", Name = "Loop" } };
        using var workflowContext = new WorkflowExecutionContext(new WorkflowType(), new Workflow { WorkflowId = "workflow" }, null, null, null, null, null, []);

        for (var i = 0; i < WorkflowExecutionContext.MaxExecutedActivities + 5; i++)
        {
            workflowContext.RecordExecution(activity, WorkflowExecutionRecordStatus.Completed, [i.ToString(CultureInfo.InvariantCulture)], DateTime.UtcNow, DateTime.UtcNow);
        }

        Assert.Equal(WorkflowExecutionContext.MaxExecutedActivities, workflowContext.ExecutedActivities.Count);
        Assert.Equal("104", workflowContext.ExecutedActivities.Peek().Outcome);
        Assert.Equal(WorkflowExecutionContext.MaxExecutedActivities + 5, workflowContext.JournalRecords.Count);
    }

    [Fact]
    public async Task StartWorkflowAsync_SavedInstance_IsNotified()
    {
        var notifier = new Mock<IWorkflowDesignerNotifier>();
        var start = new OutputTask(null, halt: false);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Activities = [new() { ActivityId = "start", IsStart = true, Name = "StartTask" }],
            Transitions = [],
        };
        var workflowManager = CreateWorkflowManager(CreateServiceProvider(), [new NamedTask("StartTask", start)], workflowType, notifier: notifier.Object);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        notifier.Verify(x => x.InstanceChangedAsync(It.Is<WorkflowInstanceChange>(change =>
            change.WorkflowId == workflowContext.Workflow.WorkflowId &&
            change.WorkflowTypeId == workflowType.WorkflowTypeId &&
            change.Status == WorkflowStatus.Finished)), Times.Once);
    }

    [Theory]
    [InlineData(WorkflowBranchingMode.FirstOnly, new[] { "start", "a" })]
    [InlineData(WorkflowBranchingMode.All, new[] { "start", "a", "b" })]
    public async Task StartWorkflowAsync_OutcomeWithTwoTransitions_FollowsThemByBranchingMode(WorkflowBranchingMode branchingMode, string[] expected)
    {
        var (journal, saved) = CreateJournal();
        var counting = new CountingTask(() => { });
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            BranchingMode = branchingMode,
            Activities =
            [
                new() { ActivityId = "start", IsStart = true, Name = "StartTask" },
                new() { ActivityId = "a", Name = counting.Name },
                new() { ActivityId = "b", Name = counting.Name },
            ],
            Transitions =
            [
                new() { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "a" },
                new() { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "b" },
            ],
        };
        var workflowManager = CreateWorkflowManager(CreateServiceProvider(), [counting, new NamedTask("StartTask", new OutputTask(null, halt: false))], workflowType, journal: journal);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        // In the All mode, the transitions run in the order they were added.
        Assert.Equal(WorkflowStatus.Finished, workflowContext.Status);
        Assert.Equal(expected, saved.Select(record => record.ActivityId));
    }

    [Theory]
    [InlineData(false, WorkflowStatus.Finished, WorkflowExecutionRecordStatus.Completed)]
    [InlineData(true, WorkflowStatus.Faulted, WorkflowExecutionRecordStatus.Faulted)]
    public async Task StartWorkflowAsync_ScriptError_IsRecordedOnItsActivityOrFaultsTheInstance(bool faultOnScriptErrors, WorkflowStatus status, WorkflowExecutionRecordStatus recordStatus)
    {
        var (journal, saved) = CreateJournal();
        var serviceProvider = CreateServiceProvider();
        var scriptTask = new ScriptTask(CreateWorkflowScriptEvaluator(serviceProvider), new PassThroughStringLocalizer<ScriptTask>());
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            FaultOnScriptErrors = faultOnScriptErrors,
            Activities =
            [
                new()
                {
                    ActivityId = "script",
                    IsStart = true,
                    Name = scriptTask.Name,
                    Properties = JObject.FromObject(new { AvailableOutcomes = new[] { "Done" }, Script = new WorkflowExpression<object>("setOutcome('Done'); throw new Error('Boom');") }),
                },
            ],
            Transitions = [],
        };
        var workflowManager = CreateWorkflowManager(serviceProvider, [scriptTask], workflowType, journal: journal);

        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(status, workflowContext.Status);
        var record = Assert.Single(saved);
        Assert.Equal("script", record.ActivityId);
        Assert.Equal(recordStatus, record.Status);
        Assert.Contains("Boom", record.Error);
    }

    [Fact]
    public async Task RetryActivityAsync_FaultedInstance_RunsAgainFromTheActivity()
    {
        var (journal, saved) = CreateJournal();
        var fail = true;
        var flaky = new FlakyTask(() => fail);
        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Activities =
            [
                new() { ActivityId = "start", IsStart = true, Name = "StartTask" },
                new() { ActivityId = "flaky", Name = flaky.Name },
            ],
            Transitions = [new() { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "flaky" }],
        };
        var workflowManager = CreateWorkflowManager(CreateServiceProvider(), [flaky, new NamedTask("StartTask", new OutputTask(null, halt: false))], workflowType, journal: journal);

        var faulted = await workflowManager.StartWorkflowAsync(workflowType);

        Assert.Equal(WorkflowStatus.Faulted, faulted.Status);

        fail = false;
        var retried = await workflowManager.RetryActivityAsync(faulted.Workflow, "flaky");

        Assert.Equal(WorkflowStatus.Finished, retried.Status);
        Assert.Null(retried.Workflow.FaultMessage);
        Assert.Equal([WorkflowExecutionRecordStatus.Completed, WorkflowExecutionRecordStatus.Faulted, WorkflowExecutionRecordStatus.Completed], saved.Select(record => record.Status));
        Assert.Equal([1, 2, 3], saved.Select(record => record.Sequence));
    }

    [Fact]
    public async Task RetryActivityAsync_InstanceNotFaultedOrUnknownActivity_Throws()
    {
        var (workflowManager, workflowType) = CreateOutputWorkflow(output: "42", bindings: []);
        var finished = await workflowManager.StartWorkflowAsync(workflowType);

        await Assert.ThrowsAsync<InvalidOperationException>(() => workflowManager.RetryActivityAsync(finished.Workflow, "output"));

        finished.Workflow.Status = WorkflowStatus.Faulted;

        await Assert.ThrowsAsync<ArgumentException>(() => workflowManager.RetryActivityAsync(finished.Workflow, "missing"));
    }

    private static (IWorkflowExecutionJournal Journal, List<WorkflowExecutionRecord> Saved) CreateJournal(bool enabled = true)
    {
        var saved = new List<WorkflowExecutionRecord>();
        var journal = new Mock<IWorkflowExecutionJournal>();
        journal.SetupGet(x => x.IsEnabled).Returns(enabled);
        journal.Setup(x => x.SaveAsync(It.IsAny<string>(), It.IsAny<IEnumerable<WorkflowExecutionRecord>>()))
            .Callback((string _, IEnumerable<WorkflowExecutionRecord> records) => saved.AddRange(records))
            .Returns(Task.CompletedTask);

        return (journal.Object, saved);
    }

    // A start activity, then an output activity setting "Value" to `output`, which halts when `halt` is set.
    private static (WorkflowManager Manager, WorkflowType WorkflowType) CreateOutputWorkflow(string output, Dictionary<string, string> bindings, bool halt = false, IWorkflowExecutionJournal journal = null)
    {
        var start = new OutputTask(null, halt: false);
        var outputTask = new OutputTask(output, halt);
        var properties = new JsonObject();
        properties.SetOutputBindings(bindings);

        var workflowType = new WorkflowType
        {
            Id = 1,
            WorkflowTypeId = IdGenerator.GenerateId(),
            Variables = [new WorkflowVariableDefinition { Name = "answer", TypeName = "number" }],
            Activities =
            [
                new() { ActivityId = "start", IsStart = true, Name = "StartTask" },
                new() { ActivityId = "output", Name = outputTask.Name, Properties = properties },
            ],
            Transitions = [new() { SourceActivityId = "start", SourceOutcomeName = "Done", DestinationActivityId = "output" }],
        };

        var serviceProvider = CreateServiceProvider();
        var workflowManager = CreateWorkflowManager(serviceProvider, [outputTask, new NamedTask("StartTask", start)], workflowType, journal: journal);

        return (workflowManager, workflowType);
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
            => Outcome(new LocalizedString("Done", "Done"));

        public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            _onExecute();

            return Task.FromResult(Outcome("Done"));
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

    // Fails while `fail` returns true.
    private sealed class FlakyTask : TaskActivity<FlakyTask>
    {
        private readonly Func<bool> _fail;

        public FlakyTask(Func<bool> fail)
        {
            _fail = fail;
        }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => _fail() ? throw new InvalidOperationException("Not ready yet") : Outcome("Done");
    }

    // Sets its "Value" output, then finishes or halts.
    private sealed class OutputTask : TaskActivity<OutputTask>, IActivityOutputs
    {
        private readonly string _output;
        private readonly bool _halt;

        public OutputTask(string output, bool halt)
        {
            _output = output;
            _halt = halt;
        }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcome(new LocalizedString("Done", "Done"));

        public IEnumerable<ActivityOutputDescriptor> GetOutputs()
            => [new ActivityOutputDescriptor { Name = "Value", TypeName = "any", DisplayName = new("Value", "Value") }];

        public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            workflowContext.SetActivityOutput(activityContext, "Value", _output);

            return _halt ? Halt() : Outcome("Done");
        }

        public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => _halt ? Halt() : Outcome("Done");
    }

    // Another activity under a different name.
    private sealed class NamedTask : TaskActivity
    {
        private readonly IActivity _inner;

        public NamedTask(string name, IActivity inner)
        {
            Name = name;
            _inner = inner;
        }

        public override string Name { get; }

        public override LocalizedString DisplayText => new(Name, Name);

        public override LocalizedString Category => new("Test", "Test");

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => _inner.GetPossibleOutcomes(workflowContext, activityContext);

        public override Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => _inner.ExecuteAsync(workflowContext, activityContext);
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
