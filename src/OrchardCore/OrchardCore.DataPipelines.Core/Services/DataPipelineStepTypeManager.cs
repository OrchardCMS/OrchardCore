using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// The default <see cref="IDataPipelineStepTypeManager"/>, built from the step types of
/// <see cref="DataPipelineStepTypeOptions"/>. It lists them from their descriptors, and resolves a step type from the
/// services of the scope only when it is used.
/// </summary>
public sealed class DataPipelineStepTypeManager : IDataPipelineStepTypeManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IReadOnlyList<DataPipelineStepTypeDescriptor> _stepTypes;
    private readonly Dictionary<string, DataPipelineStepTypeDescriptor> _byName = new(StringComparer.Ordinal);

    public DataPipelineStepTypeManager(IOptions<DataPipelineStepTypeOptions> options, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);

        _serviceProvider = serviceProvider;
        _stepTypes = options.Value.StepTypes;

        foreach (var stepType in _stepTypes)
        {
            _byName.TryAdd(stepType.Name, stepType);
        }
    }

    public IReadOnlyList<DataPipelineStepTypeDescriptor> GetStepTypes() => _stepTypes;

    public DataPipelineStepTypeDescriptor GetDescriptor(string name)
    {
        return string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);
    }

    public IDataPipelineStepType GetStepType(string name)
    {
        return GetDescriptor(name) is { } descriptor
            ? (IDataPipelineStepType)_serviceProvider.GetRequiredService(descriptor.Type)
            : null;
    }
}
