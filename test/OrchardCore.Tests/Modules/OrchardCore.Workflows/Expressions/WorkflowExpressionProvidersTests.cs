using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using OrchardCore.Liquid;
using OrchardCore.Scripting;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;

public sealed class WorkflowExpressionProvidersTests
{
    private static readonly LiteralExpressionProvider s_literal = new(new PassThroughStringLocalizer<LiteralExpressionProvider>());

    [Fact]
    public async Task Literal_Values_ConvertToTheExpectedType()
    {
        Assert.Equal("hello", await EvaluateLiteralAsync<string>("hello"));
        Assert.True(await EvaluateLiteralAsync<bool>(" true "));
        Assert.Equal(42, await EvaluateLiteralAsync<int>("42"));
        Assert.Equal(4.5, await EvaluateLiteralAsync<double>("4.5"));
        Assert.Equal(["a", "b"], await EvaluateLiteralAsync<IEnumerable<object>>("a, b"));
        var list = Assert.IsType<List<object>>(await EvaluateLiteralAsync<IEnumerable<object>>("[1, \"x\"]"));
        Assert.Equal(1d, Convert.ToDouble(list[0], CultureInfo.InvariantCulture));
        Assert.Equal("x", list[1]);
        Assert.Equal("{\"a\":1}", (await EvaluateLiteralAsync<JsonObject>("{\"a\":1}")).ToJsonString());
    }

    [Fact]
    public async Task Literal_Empty_IsTheDefaultValue()
    {
        Assert.False(await EvaluateLiteralAsync<bool>(""));
        Assert.Null(await EvaluateLiteralAsync<int?>(" "));
        Assert.Equal("", await EvaluateLiteralAsync<string>(""));
    }

    [Fact]
    public async Task Literal_ValueThatDoesNotConvert_ThrowsAndFailsValidation()
    {
        await Assert.ThrowsAsync<FormatException>(() => EvaluateLiteralAsync<bool>("many"));

        Assert.Single(s_literal.Validate("many", typeof(bool)));
        Assert.Single(s_literal.Validate("1.2.3", typeof(double)));
        Assert.Single(s_literal.Validate("[1,", typeof(IEnumerable<object>)));
        Assert.Empty(s_literal.Validate("anything", typeof(object)));
    }

    [Fact]
    public async Task Liquid_Expression_IsEvaluatedByTheLiquidEvaluatorWithTheEncoder()
    {
        var evaluator = new Mock<IWorkflowExpressionEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<string>>(), It.IsAny<WorkflowExecutionContext>(), HtmlEncoder.Default)).ReturnsAsync("rendered");
        var templateManager = new Mock<ILiquidTemplateManager>();
        IEnumerable<string> errors = ["Unexpected end"];
        templateManager.Setup(x => x.Validate("{{ x", out errors)).Returns(false);
        var provider = new LiquidExpressionProvider(evaluator.Object, templateManager.Object, new PassThroughStringLocalizer<LiquidExpressionProvider>());

        var value = await provider.EvaluateAsync(new WorkflowExpression<string>("{{ x }}"), null, new WorkflowExpressionEvaluationContext { Encoder = HtmlEncoder.Default });

        Assert.Equal("rendered", value);
        Assert.Contains("Unexpected end", Assert.Single(provider.Validate("{{ x", typeof(string))));
        Assert.Equal("liquid", provider.EditorLanguage);
    }

    [Fact]
    public async Task JavaScript_Expression_IsEvaluatedByTheScriptEvaluatorWithTheMethodProviders()
    {
        var methods = new[] { Mock.Of<IGlobalMethodProvider>() };
        var evaluator = new Mock<IWorkflowScriptEvaluator>();
        evaluator.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<bool>>(), It.IsAny<WorkflowExecutionContext>(), methods)).ReturnsAsync(true);
        var provider = new JavaScriptExpressionProvider(evaluator.Object, new PassThroughStringLocalizer<JavaScriptExpressionProvider>());

        Assert.True(await provider.EvaluateAsync(new WorkflowExpression<bool>("1 < 2"), null, new WorkflowExpressionEvaluationContext { MethodProviders = methods }));
        Assert.Empty(provider.Validate("((", typeof(bool)));
    }

    [Fact]
    public void Manager_Providers_AreListedBuiltInFirstAndFoundIgnoringCase()
    {
        var custom = CustomProvider("Upper");
        var replacement = CustomProvider(WorkflowExpressionSyntaxes.Literal);
        var manager = new WorkflowExpressionManager([custom, s_literal, CustomProvider(WorkflowExpressionSyntaxes.JavaScript), replacement]);

        Assert.Equal(["Literal", "JavaScript", "Upper"], manager.List().Select(provider => provider.Name));
        Assert.Same(custom, manager.Get("upper"));
        Assert.Same(replacement, manager.Get("literal"));
        Assert.Null(manager.Get("Python"));
        Assert.Null(manager.Get(null));
    }

    [Fact]
    public async Task Manager_EvaluateAsync_UsesTheSyntaxOfTheExpressionOrTheDefault()
    {
        var manager = new WorkflowExpressionManager([s_literal, CustomProvider("Upper")]);

        Assert.Equal("ABC", await manager.EvaluateAsync(new WorkflowExpression<string>("abc", "Upper"), null, WorkflowExpressionSyntaxes.Literal));
        Assert.Equal("abc", await manager.EvaluateAsync(new WorkflowExpression<string>("abc"), null, WorkflowExpressionSyntaxes.Literal));
        await Assert.ThrowsAsync<NotSupportedException>(() => manager.EvaluateAsync(new WorkflowExpression<string>("abc", "Python"), null));
    }

    [Fact]
    public void Json_ExpressionWithoutSyntax_IsStoredAsBefore()
    {
        var legacy = JObject.FromObject(new WorkflowExpression<bool>("isValid"));
        var withSyntax = JObject.FromObject(new WorkflowExpression<bool>("isValid", WorkflowExpressionSyntaxes.JavaScript));

        Assert.Equal("{\"Expression\":\"isValid\"}", legacy.ToJsonString());
        Assert.Equal("JavaScript", withSyntax["Syntax"].GetValue<string>());
        Assert.Null(legacy.ToObject<WorkflowExpression<bool>>().Syntax);
    }

    [Theory]
    [InlineData(null, WorkflowScriptSyntax.JavaScript, "js", "JavaScript")]
    [InlineData(null, WorkflowScriptSyntax.Liquid, "liquid", "Liquid")]
    [InlineData("Literal", WorkflowScriptSyntax.Liquid, "js", "Literal")]
    public void Resolve_LegacyOrNewShape_ReturnsTheExpressionToEvaluate(string syntax, WorkflowScriptSyntax legacySyntax, string expectedText, string expectedSyntax)
    {
        var resolved = WorkflowExpressionSyntaxes.Resolve(new WorkflowExpression<bool>("js", syntax), "liquid", legacySyntax);

        Assert.Equal(expectedText, resolved.Expression);
        Assert.Equal(expectedSyntax, resolved.Syntax);
    }

    private static Task<T> EvaluateLiteralAsync<T>(string text)
        => s_literal.EvaluateAsync(new WorkflowExpression<T>(text, WorkflowExpressionSyntaxes.Literal), null, null);

    private static IWorkflowExpressionProvider CustomProvider(string name)
    {
        var provider = new Mock<IWorkflowExpressionProvider>();
        provider.SetupGet(x => x.Name).Returns(name);
        provider.Setup(x => x.EvaluateAsync(It.IsAny<WorkflowExpression<string>>(), It.IsAny<WorkflowExecutionContext>(), It.IsAny<WorkflowExpressionEvaluationContext>()))
            .Returns((WorkflowExpression<string> expression, WorkflowExecutionContext _, WorkflowExpressionEvaluationContext _) => Task.FromResult(expression.Expression.ToUpperInvariant()));

        return provider.Object;
    }
}
