using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class WorkflowVariableValidatorTests
{
    private readonly WorkflowVariableValidator _validator = new(
        new WorkflowVariableTypeProvider(
        [
            new StringVariableType(new PassThroughStringLocalizer<StringVariableType>()),
            new NumberVariableType(new PassThroughStringLocalizer<NumberVariableType>()),
        ]),
        new PassThroughStringLocalizer<WorkflowVariableValidator>());

    [Fact]
    public void Validate_ValidDeclarations_ReturnsNoErrors()
    {
        var errors = _validator.Validate(
        [
            new WorkflowVariableDefinition { Name = "greeting", TypeName = "string", DefaultValue = "Hello" },
            new WorkflowVariableDefinition { Name = "_count2", TypeName = "NUMBER", DefaultValue = 3 },
        ]);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("", "string", null, "Enter a name.")]
    [InlineData("2fast", "string", null, "isn't a valid name")]
    [InlineData("has space", "string", null, "isn't a valid name")]
    [InlineData("count", "unknown", null, "The type 'unknown' isn't available.")]
    [InlineData("count", "number", "\"many\"", "The default value of 'count'")]
    public void Validate_InvalidDeclaration_ReturnsItsError(string name, string typeName, string defaultValue, string message)
    {
        var error = Assert.Single(_validator.Validate(
        [
            new WorkflowVariableDefinition { Name = name, TypeName = typeName, DefaultValue = defaultValue is null ? null : JsonNode.Parse(defaultValue) },
        ]));

        Assert.Equal(0, error.Index);
        Assert.Contains(message, error.Message);
    }

    [Fact]
    public void Validate_SameNameIgnoringCase_ReportsTheSecond()
    {
        var error = Assert.Single(_validator.Validate(
        [
            new WorkflowVariableDefinition { Name = "total", TypeName = "number" },
            new WorkflowVariableDefinition { Name = "Total", TypeName = "number" },
        ]));

        Assert.Equal(1, error.Index);
        Assert.Contains("Another variable is named 'Total'.", error.Message);
    }

    [Fact]
    public void TypeProvider_LaterRegistration_ReplacesTheType()
    {
        var replacement = Mock.Of<IWorkflowVariableType>(type => type.Name == "string");
        var provider = new WorkflowVariableTypeProvider([new StringVariableType(new PassThroughStringLocalizer<StringVariableType>()), replacement]);

        Assert.Same(replacement, provider.Get("STRING"));
        Assert.Single(provider.List());
        Assert.Null(provider.Get("missing"));
    }
}
