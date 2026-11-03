using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DisplayManagement.Handlers;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers step types with the display drivers of their editors.
/// </summary>
public static class DataPipelineStepServiceCollectionExtensions
{
    /// <summary>
    /// Registers a step type and the display driver that renders its editor in the designer.
    /// </summary>
    /// <typeparam name="TStepType">The step type.</typeparam>
    /// <typeparam name="TDriver">The display driver of its editor.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddDataPipelineStep<TStepType, TDriver>(this IServiceCollection services)
        where TStepType : class, IDataPipelineStepType
        where TDriver : class, IDisplayDriver<DataPipelineStep>
    {
        services.AddDataPipelineStepType<TStepType>();
        services.AddDisplayDriver<DataPipelineStep, TDriver>();

        return services;
    }
}
