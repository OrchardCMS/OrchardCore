using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the data pipeline engine and step types.
/// </summary>
public static class DataPipelinesServiceCollectionExtensions
{
    /// <summary>
    /// Registers the step type manager, the analyzer and the executor.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataPipelinesCore(this IServiceCollection services)
    {
        services.TryAddScoped<IDataPipelineStepTypeManager, DataPipelineStepTypeManager>();
        services.TryAddScoped<DataPipelineAnalyzer>();
        services.TryAddScoped<DataPipelineExecutor>();

        return services;
    }

    /// <summary>
    /// Registers a step type: its descriptor, built from its static members, and its class, as a scoped service.
    /// </summary>
    /// <typeparam name="TStepType">The step type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataPipelineStepType<TStepType>(this IServiceCollection services)
        where TStepType : class, IDataPipelineStepType
    {
        services.TryAddScoped<TStepType>();
        services.Configure<DataPipelineStepTypeOptions>(options => options.Add(DataPipelineStepTypeDescriptor.Create<TStepType>()));

        return services;
    }
}
