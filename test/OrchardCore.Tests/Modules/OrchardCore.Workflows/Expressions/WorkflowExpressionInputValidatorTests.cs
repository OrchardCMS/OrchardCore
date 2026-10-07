using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Liquid;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;

public sealed class WorkflowExpressionInputValidatorTests
{
    private readonly ModelStateDictionary _modelState = new();
    private readonly WorkflowExpressionInputValidator _validator;

    public WorkflowExpressionInputValidatorTests()
    {
        var templateManager = new Mock<ILiquidTemplateManager>();
        IEnumerable<string> errors = ["Unexpected end"];
        templateManager.Setup(x => x.Validate("{{ x", out errors)).Returns(false);
        var manager = new WorkflowExpressionManager(
        [
            new LiteralExpressionProvider(new PassThroughStringLocalizer<LiteralExpressionProvider>()),
            new LiquidExpressionProvider(Mock.Of<IWorkflowExpressionEvaluator>(), templateManager.Object, new PassThroughStringLocalizer<LiquidExpressionProvider>()),
            new JavaScriptExpressionProvider(Mock.Of<IWorkflowScriptEvaluator>(), new PassThroughStringLocalizer<JavaScriptExpressionProvider>()),
        ]);
        _validator = new WorkflowExpressionInputValidator(manager, new PassThroughStringLocalizer<WorkflowExpressionInputValidator>());
    }

    [Fact]
    public void Validate_ValidInput_ReturnsTheExpressionWithTheRegisteredSyntaxName()
    {
        var expression = _validator.Validate<bool>(Input("1 < 2", "javascript"), _modelState, "Task", "Condition", new() { Label = "Condition", Required = true });

        Assert.True(_modelState.IsValid);
        Assert.Equal("1 < 2", expression.Expression);
        Assert.Equal(WorkflowExpressionSyntaxes.JavaScript, expression.Syntax);
    }

    [Fact]
    public void Validate_NoSyntax_UsesTheDefault()
    {
        var expression = _validator.Validate<string>(Input("hello", null), _modelState, "Task", "Value", new() { Label = "Value", DefaultSyntax = WorkflowExpressionSyntaxes.Literal });

        Assert.Equal(WorkflowExpressionSyntaxes.Literal, expression.Syntax);
    }

    [Fact]
    public void Validate_EmptyRequiredInput_AddsAnErrorOnTheExpression()
    {
        _validator.Validate<bool>(Input(" ", "JavaScript"), _modelState, "Task", "Condition", new() { Label = "Condition", Required = true });

        Assert.Equal("Condition is required.", Assert.Single(_modelState["Task.Condition.Expression"].Errors).ErrorMessage);
    }

    [Fact]
    public void Validate_InvalidLiquidAndLiteral_AddTheProviderErrors()
    {
        _validator.Validate<string>(Input("{{ x", "Liquid"), _modelState, "Task", "Value", new() { Label = "Value" });
        _validator.Validate<bool>(Input("maybe", "Literal"), _modelState, "Task", "Condition", new() { Label = "Condition" });

        Assert.Contains("Unexpected end", Assert.Single(_modelState["Task.Value.Expression"].Errors).ErrorMessage);
        Assert.Single(_modelState["Task.Condition.Expression"].Errors);
    }

    [Fact]
    public void Validate_UnknownOrDisallowedSyntax_AddsAnErrorOnTheSyntax()
    {
        _validator.Validate<string>(Input("x", "Python"), _modelState, "Task", "A", new() { Label = "A" });
        _validator.Validate<string>(Input("x", "Liquid"), _modelState, "Task", "B", new() { Label = "B", Syntaxes = [WorkflowExpressionSyntaxes.JavaScript] });

        Assert.Equal("The Python syntax isn't available.", Assert.Single(_modelState["Task.A.Syntax"].Errors).ErrorMessage);
        Assert.Equal("B can't be written in Liquid.", Assert.Single(_modelState["Task.B.Syntax"].Errors).ErrorMessage);
    }

    private static WorkflowExpressionInput Input(string expression, string syntax)
        => new() { Expression = expression, Syntax = syntax };
}
