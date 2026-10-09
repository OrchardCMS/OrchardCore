using OrchardCore.Scripting;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;

// The activities whose expressions have a syntax each: the new shape with each syntax, and the legacy pairs.
public sealed class ExpressionActivitiesTests
{
    private readonly Mock<IWorkflowScriptEvaluator> _scriptEvaluator = new();
    private readonly Mock<IWorkflowExpressionEvaluator> _liquidEvaluator = new();

    [Theory]
    [InlineData("true", "True")]
    [InlineData("false", "False")]
    public async Task IfElse_LiteralCondition_TakesItsOutcome(string condition, string outcome)
    {
        var task = new IfElseTask(CreateManager(), Localizer<IfElseTask>())
        {
            Condition = new WorkflowExpression<bool>(condition, WorkflowExpressionSyntaxes.Literal),
        };

        using var context = CreateContext();

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal([outcome], result.Outcomes);
    }

    [Fact]
    public async Task IfElse_ConditionWithASyntax_IsUsedInsteadOfTheLegacyPair()
    {
        _liquidEvaluator.Setup(x => x.EvaluateAsync(It.Is<WorkflowExpression<bool>>(e => e.Expression == "{{ new }}"), It.IsAny<WorkflowExecutionContext>(), null)).ReturnsAsync(true);
        var task = new IfElseTask(CreateManager(), Localizer<IfElseTask>())
        {
            Syntax = WorkflowScriptSyntax.JavaScript,
            Condition = new WorkflowExpression<bool>("{{ new }}", WorkflowExpressionSyntaxes.Liquid),
            LiquidCondition = new WorkflowExpression<bool>("{{ legacy }}"),
        };

        using var context = CreateContext();

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(["True"], result.Outcomes);
        _scriptEvaluator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task IfElse_LegacyJavaScript_EvaluatesTheCondition()
    {
        _scriptEvaluator.Setup(x => x.EvaluateAsync(It.Is<WorkflowExpression<bool>>(e => e.Expression == "1 < 2"), It.IsAny<WorkflowExecutionContext>(), It.IsAny<IGlobalMethodProvider[]>())).ReturnsAsync(true);
        var task = new IfElseTask(CreateManager(), Localizer<IfElseTask>())
        {
            Condition = new WorkflowExpression<bool>("1 < 2"),
        };

        using var context = CreateContext();

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(["True"], result.Outcomes);
    }

    [Fact]
    public async Task WhileLoop_LiteralCondition_IsDone()
    {
        var task = new WhileLoopTask(CreateManager(), Localizer<WhileLoopTask>())
        {
            Condition = new WorkflowExpression<bool>("false", WorkflowExpressionSyntaxes.Literal),
        };

        using var context = CreateContext();

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(["Done"], result.Outcomes);
    }

    [Fact]
    public async Task ForLoop_LiteralBounds_Iterate()
    {
        using var context = CreateContext();
        var task = new ForLoopTask(CreateManager(), Localizer<ForLoopTask>())
        {
            From = new WorkflowExpression<double>("2", WorkflowExpressionSyntaxes.Literal),
            To = new WorkflowExpression<double>("4", WorkflowExpressionSyntaxes.Literal),
            Step = new WorkflowExpression<double>("1", WorkflowExpressionSyntaxes.Literal),
        };

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(["Iterate"], result.Outcomes);
        Assert.Equal(2d, context.Properties["x"]);
    }

    [Fact]
    public async Task ForLoop_LegacyNumbers_AreReadWithoutAScript()
    {
        using var context = CreateContext();
        var task = new ForLoopTask(CreateManager(), Localizer<ForLoopTask>())
        {
            From = new WorkflowExpression<double>("0"),
            To = new WorkflowExpression<double>("0"),
            Step = new WorkflowExpression<double>("1"),
        };

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(["Done"], result.Outcomes);
        _scriptEvaluator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ForEach_LiteralList_IteratesItsItems()
    {
        using var context = CreateContext();
        var task = new ForEachTask(CreateManager(), Localizer<ForEachTask>())
        {
            Enumerable = new WorkflowExpression<IEnumerable<object>>("a, b", WorkflowExpressionSyntaxes.Literal),
        };

        var result = await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(["Iterate"], result.Outcomes);
        Assert.Equal("a", context.Properties["x"]);
    }

    [Fact]
    public async Task ForEach_LiquidListWithASyntax_ReadsTheRenderedJson()
    {
        _liquidEvaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), It.IsAny<WorkflowExecutionContext>(), null)).ReturnsAsync("[\"x\", \"y\"]");
        using var context = CreateContext();
        var task = new ForEachTask(CreateManager(), Localizer<ForEachTask>())
        {
            Enumerable = new WorkflowExpression<IEnumerable<object>>("{{ items | json }}", WorkflowExpressionSyntaxes.Liquid),
        };

        await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal("x", context.Properties["x"]?.ToString());
    }

    [Fact]
    public async Task SetOutputAndSetProperty_LiteralValues_AreStored()
    {
        using var context = CreateContext();
        var setOutput = new SetOutputTask(CreateManager(), Localizer<SetOutputTask>())
        {
            OutputName = "Message",
            Value = new WorkflowExpression<object>("hello", WorkflowExpressionSyntaxes.Literal),
        };
        var setProperty = new SetPropertyTask(CreateManager(), Localizer<SetPropertyTask>())
        {
            PropertyName = "Name",
            Value = new WorkflowExpression<object>("world", WorkflowExpressionSyntaxes.Literal),
        };

        await setOutput.ExecuteAsync(context, new ActivityContext());
        await setProperty.ExecuteAsync(context, new ActivityContext());

        Assert.Equal("hello", context.Output["Message"]);
        Assert.Equal("world", context.Properties["Name"]);
    }

    [Fact]
    public async Task SetVariable_LiteralValue_IsConvertedToTheVariableType()
    {
        var task = new SetVariableTask(CreateManager(), Localizer<SetVariableTask>())
        {
            VariableName = "count",
            Value = new WorkflowExpression<object>("41", WorkflowExpressionSyntaxes.Literal),
        };
        using var context = new WorkflowExecutionContext(
            new WorkflowType { Variables = [new WorkflowVariableDefinition { Name = "count", TypeName = "number" }] },
            new Workflow { WorkflowId = "workflow" },
            null,
            null,
            null,
            null,
            null,
            [],
            TestVariableTypes.CreateProvider());

        await task.ExecuteAsync(context, new ActivityContext());

        Assert.Equal(41d, context.Properties["count"]);
    }

    [Fact]
    public async Task Correlate_LegacyLiquidAndLiteral_SetTheCorrelationId()
    {
        _liquidEvaluator.Setup(x => x.EvaluateAsync(It.Is<WorkflowExpression<string>>(e => e.Expression == "{{ id }}"), It.IsAny<WorkflowExecutionContext>(), null)).ReturnsAsync(" from-liquid ");
        using var context = CreateContext();
        var legacy = new CorrelateTask(CreateManager(), Localizer<CorrelateTask>())
        {
            Syntax = WorkflowScriptSyntax.Liquid,
            Value = new WorkflowExpression<string>("{{ id }}"),
        };

        await legacy.ExecuteAsync(context, new ActivityContext());

        Assert.Equal("from-liquid", context.CorrelationId);

        var literal = new CorrelateTask(CreateManager(), Localizer<CorrelateTask>())
        {
            Value = new WorkflowExpression<string>(" order-1 ", WorkflowExpressionSyntaxes.Literal),
        };

        await literal.ExecuteAsync(context, new ActivityContext());

        Assert.Equal("order-1", context.CorrelationId);
    }

    private IWorkflowExpressionManager CreateManager()
        => TestExpressions.CreateManager(_scriptEvaluator.Object, _liquidEvaluator.Object);

    private static PassThroughStringLocalizer<T> Localizer<T>()
        => new();

    private static WorkflowExecutionContext CreateContext()
        => new(new WorkflowType(), new Workflow { WorkflowId = "workflow" }, null, null, null, null, null, []);
}
