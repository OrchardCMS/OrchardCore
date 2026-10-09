using OrchardCore.Contents.Workflows.Variables;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

/// <summary>
/// The built-in variable types, and the content item type, for tests that run workflows.
/// </summary>
internal static class TestVariableTypes
{
    public static IWorkflowVariableTypeProvider CreateProvider()
        => new WorkflowVariableTypeProvider(
        [
            new StringVariableType(new PassThroughStringLocalizer<StringVariableType>()),
            new NumberVariableType(new PassThroughStringLocalizer<NumberVariableType>()),
            new BooleanVariableType(new PassThroughStringLocalizer<BooleanVariableType>()),
            new DateTimeVariableType(new PassThroughStringLocalizer<DateTimeVariableType>()),
            new ObjectVariableType(new PassThroughStringLocalizer<ObjectVariableType>()),
            new ArrayVariableType(new PassThroughStringLocalizer<ArrayVariableType>()),
            new AnyVariableType(new PassThroughStringLocalizer<AnyVariableType>()),
            new ContentItemVariableType(new PassThroughStringLocalizer<ContentItemVariableType>()),
        ]);
}
