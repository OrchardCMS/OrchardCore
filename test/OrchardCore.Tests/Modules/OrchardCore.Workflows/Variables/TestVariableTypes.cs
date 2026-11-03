using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Contents.Workflows.Variables;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Variables;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;

/// <summary>
/// The built-in variable types, and the content item type, for tests that run workflows.
/// </summary>
internal static class TestVariableTypes
{
    public static IWorkflowVariableTypeProvider CreateProvider()
    {
        return Create(services => services
            .AddWorkflowVariableType<StringVariableType>()
            .AddWorkflowVariableType<NumberVariableType>()
            .AddWorkflowVariableType<BooleanVariableType>()
            .AddWorkflowVariableType<DateTimeVariableType>()
            .AddWorkflowVariableType<ObjectVariableType>()
            .AddWorkflowVariableType<ArrayVariableType>()
            .AddWorkflowVariableType<AnyVariableType>()
            .AddWorkflowVariableType<ContentItemVariableType>());
    }

    /// <summary>
    /// Returns a provider of the types <paramref name="register"/> registers.
    /// </summary>
    public static IWorkflowVariableTypeProvider Create(Action<IServiceCollection> register)
    {
        var services = new ServiceCollection();
        register(services);
        services.AddSingleton<IWorkflowVariableTypeProvider, WorkflowVariableTypeProvider>();

        return services.BuildServiceProvider().GetRequiredService<IWorkflowVariableTypeProvider>();
    }
}
