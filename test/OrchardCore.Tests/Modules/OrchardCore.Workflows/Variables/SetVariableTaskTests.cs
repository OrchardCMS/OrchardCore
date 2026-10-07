using OrchardCore.Scripting;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class SetVariableTaskTests
{
    private readonly Mock<IWorkflowScriptEvaluator> _scriptEvaluator = new();
    private readonly Mock<IWorkflowExpressionEvaluator> _expressionEvaluator = new();

    [Fact]
    public async Task ExecuteAsync_JavaScriptValue_SetsTheDeclaredVariableWithItsType()
    {
        ScriptReturns("41");
        var task = CreateTask("count", WorkflowScriptSyntax.JavaScript);
        var (workflowContext, activityContext) = CreateContext(task, new WorkflowVariableDefinition { Name = "count", TypeName = "number" });

        var result = await task.ExecuteAsync(workflowContext, activityContext);

        Assert.Contains("Done", result.Outcomes);
        Assert.Equal(41d, workflowContext.Properties["count"]);
    }

    [Fact]
    public async Task ExecuteAsync_LiquidValue_SetsTheVariable()
    {
        _expressionEvaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), It.IsAny<WorkflowExecutionContext>(), null))
            .ReturnsAsync("Hello");
        var task = CreateTask("greeting", WorkflowScriptSyntax.Liquid);
        var (workflowContext, activityContext) = CreateContext(task, new WorkflowVariableDefinition { Name = "greeting", TypeName = "string" });

        await task.ExecuteAsync(workflowContext, activityContext);

        Assert.Equal("Hello", workflowContext.Variables["greeting"]);
    }

    [Fact]
    public async Task ExecuteAsync_UndeclaredName_SetsTheWorkflowPropertyAsItIs()
    {
        var value = new { A = 1 };
        _scriptEvaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), It.IsAny<WorkflowExecutionContext>(), It.IsAny<IGlobalMethodProvider[]>()))
            .ReturnsAsync(value);
        var task = CreateTask("anything", WorkflowScriptSyntax.JavaScript);
        var (workflowContext, activityContext) = CreateContext(task);

        await task.ExecuteAsync(workflowContext, activityContext);

        Assert.Same(value, workflowContext.Properties["anything"]);
    }

    [Fact]
    public async Task ExecuteAsync_ValueOfTheWrongType_Throws()
    {
        ScriptReturns("many");
        var task = CreateTask("count", WorkflowScriptSyntax.JavaScript);
        var (workflowContext, activityContext) = CreateContext(task, new WorkflowVariableDefinition { Name = "count", TypeName = "number" });

        var exception = await Assert.ThrowsAsync<WorkflowVariableException>(() => task.ExecuteAsync(workflowContext, activityContext));

        Assert.Equal("count", exception.VariableName);
    }

    private void ScriptReturns(object value)
        => _scriptEvaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), It.IsAny<WorkflowExecutionContext>(), It.IsAny<IGlobalMethodProvider[]>()))
            .ReturnsAsync(value);

    private SetVariableTask CreateTask(string variableName, WorkflowScriptSyntax syntax)
        => new(_scriptEvaluator.Object, _expressionEvaluator.Object, new PassThroughStringLocalizer<SetVariableTask>())
        {
            VariableName = variableName,
            Syntax = syntax,
            Value = new WorkflowExpression<object>("value"),
            LiquidValue = new WorkflowExpression<object>("{{ value }}"),
        };

    private static (WorkflowExecutionContext WorkflowContext, ActivityContext ActivityContext) CreateContext(SetVariableTask activity, params WorkflowVariableDefinition[] variables)
    {
        var record = new ActivityRecord { ActivityId = "set", Name = activity.Name, Properties = activity.Properties };
        var activityContext = new ActivityContext { ActivityRecord = record, Activity = activity };
        var workflowContext = new WorkflowExecutionContext(
            new WorkflowType { Variables = variables },
            new Workflow { WorkflowId = "workflow" },
            null,
            null,
            null,
            null,
            null,
            [activityContext],
            TestVariableTypes.CreateProvider());

        return (workflowContext, activityContext);
    }
}
