using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Workflows.Activities;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class ActivityOutputsTests
{
    public static TheoryData<IActivityOutputs, string> DeclaredOutputs => new()
    {
        { new ScriptTask(Mock.Of<IWorkflowScriptEvaluator>(), new PassThroughStringLocalizer<ScriptTask>()), "Result:any" },
        { new LiquidTask(Mock.Of<IWorkflowExpressionEvaluator>(), new PassThroughStringLocalizer<LiquidTask>()), "Result:string" },
        { new SetPropertyTask(Mock.Of<IWorkflowScriptEvaluator>(), Mock.Of<IWorkflowExpressionEvaluator>(), new PassThroughStringLocalizer<SetPropertyTask>()), "Value:any" },
        {
            new HttpRequestTask(Mock.Of<IWorkflowExpressionEvaluator>(), UrlEncoder.Default, Mock.Of<IHttpClientFactory>(), new PassThroughStringLocalizer<HttpRequestTask>()),
            "Body:string,StatusCode:number,Response:object"
        },
        {
            new CreateContentTask(Mock.Of<IContentManager>(), Mock.Of<IWorkflowExpressionEvaluator>(), Mock.Of<IWorkflowScriptEvaluator>(), new PassThroughStringLocalizer<CreateContentTask>(), JavaScriptEncoder.Default, Mock.Of<ISession>()),
            "ContentItem:contentItem"
        },
        {
            new UpdateContentTask(Mock.Of<IContentManager>(), Mock.Of<IUpdateModelAccessor>(), Mock.Of<IWorkflowExpressionEvaluator>(), Mock.Of<IWorkflowScriptEvaluator>(), new PassThroughStringLocalizer<UpdateContentTask>(), JavaScriptEncoder.Default, Mock.Of<ISession>()),
            "ContentItem:contentItem"
        },
        { new RetrieveContentTask(Mock.Of<IContentManager>(), Mock.Of<IWorkflowScriptEvaluator>(), new PassThroughStringLocalizer<RetrieveContentTask>()), "ContentItem:contentItem" },
    };

    [Theory]
    [MemberData(nameof(DeclaredOutputs))]
    public void GetOutputs_ValueProducingActivity_DeclaresItsOutputs(IActivityOutputs activity, string expected)
    {
        Assert.Equal(expected, string.Join(",", activity.GetOutputs().Select(output => $"{output.Name}:{output.TypeName}")));
        Assert.All(activity.GetOutputs(), output => Assert.False(string.IsNullOrEmpty(output.DisplayName)));
    }

    [Fact]
    public async Task ExecuteAsync_ScriptTask_SetsItsResultOutput()
    {
        var evaluator = new Mock<IWorkflowScriptEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), It.IsAny<WorkflowExecutionContext>(), It.IsAny<IGlobalMethodProvider[]>()))
            .ReturnsAsync(42d);
        var task = new ScriptTask(evaluator.Object, new PassThroughStringLocalizer<ScriptTask>()) { Script = new WorkflowExpression<object>("21 * 2") };
        var (workflowContext, activityContext) = CreateContext(task);

        await task.ExecuteAsync(workflowContext, activityContext);

        Assert.Equal(42d, workflowContext.GetActivityOutputs("activity")["Result"]);
    }

    [Fact]
    public async Task ExecuteAsync_SetPropertyTask_SetsItsValueOutput()
    {
        var evaluator = new Mock<IWorkflowScriptEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<object>>(), It.IsAny<WorkflowExecutionContext>(), It.IsAny<IGlobalMethodProvider[]>()))
            .ReturnsAsync("value");
        var task = new SetPropertyTask(evaluator.Object, Mock.Of<IWorkflowExpressionEvaluator>(), new PassThroughStringLocalizer<SetPropertyTask>())
        {
            PropertyName = "name",
            Syntax = WorkflowScriptSyntax.JavaScript,
            Value = new WorkflowExpression<object>("'value'"),
        };
        var (workflowContext, activityContext) = CreateContext(task);

        await task.ExecuteAsync(workflowContext, activityContext);

        Assert.Equal("value", workflowContext.Properties["name"]);
        Assert.Equal("value", workflowContext.GetActivityOutputs("activity")["Value"]);
    }

    [Fact]
    public void OutputBindings_SetAndGet_KeepOnlyNamedVariables()
    {
        var properties = new JsonObject { ["Other"] = 1 };

        properties.SetOutputBindings(new Dictionary<string, string> { ["Result"] = "answer", ["Body"] = "", ["Status"] = " " });

        Assert.Equal(new Dictionary<string, string> { ["Result"] = "answer" }, properties.GetOutputBindings());
        Assert.Equal(1, properties["Other"]!.GetValue<int>());

        properties.SetOutputBindings(new Dictionary<string, string>());

        Assert.False(properties.ContainsKey(ActivityOutputBindingExtensions.PropertyName));
        Assert.Empty(properties.GetOutputBindings());
    }

    private static (WorkflowExecutionContext WorkflowContext, ActivityContext ActivityContext) CreateContext(IActivity activity)
    {
        var record = new ActivityRecord { ActivityId = "activity", Name = activity.Name, Properties = activity.Properties };
        var activityContext = new ActivityContext { ActivityRecord = record, Activity = activity };
        var workflowContext = new WorkflowExecutionContext(new WorkflowType(), new Workflow { WorkflowId = "workflow" }, null, null, null, null, null, [activityContext]);

        return (workflowContext, activityContext);
    }
}
