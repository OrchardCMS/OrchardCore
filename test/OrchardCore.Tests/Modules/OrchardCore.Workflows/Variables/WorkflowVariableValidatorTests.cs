using System.Text.Json.Nodes;
using OrchardCore.Localization;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

public sealed class WorkflowVariableValidatorTests
{
    private readonly WorkflowVariableValidator _validator = new(
        TestVariableTypes.Create(services => services
            .AddWorkflowVariableType<StringVariableType>()
            .AddWorkflowVariableType<NumberVariableType>()),
        new PassThroughStringLocalizerFactory(),
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
        var provider = TestVariableTypes.Create(services => services
            .AddWorkflowVariableType<StringVariableType>()
            .AddWorkflowVariableType<ReplacementStringType>());

        Assert.IsType<ReplacementStringType>(provider.Get("STRING"));
        Assert.Equal("Short text", Assert.Single(provider.List()).DisplayName.Value);
        Assert.Null(provider.Get("missing"));
        Assert.Null(provider.GetDescriptor("missing"));
    }

    private sealed class ReplacementStringType : IWorkflowVariableType
    {
        public static string Name => "string";

        public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<ReplacementStringType>("Short text");

        public static string Editor => "text";

        public bool TryCoerce(object value, out object result)
        {
            result = value;

            return true;
        }
    }
}
