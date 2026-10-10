using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// The default <see cref="IDataPipelineStepTypeManager"/>, built from the registered step types.
/// </summary>
public sealed class DataPipelineStepTypeManager : IDataPipelineStepTypeManager
{
    private readonly IReadOnlyList<IDataPipelineStepType> _stepTypes;
    private readonly Dictionary<string, IDataPipelineStepType> _byName = new(StringComparer.Ordinal);

    public DataPipelineStepTypeManager(IEnumerable<IDataPipelineStepType> stepTypes)
    {
        ArgumentNullException.ThrowIfNull(stepTypes);

        _stepTypes = stepTypes.ToArray();

        foreach (var stepType in _stepTypes)
        {
            _byName.TryAdd(stepType.Name, stepType);
        }
    }

    public IReadOnlyList<IDataPipelineStepType> GetStepTypes() => _stepTypes;

    public IDataPipelineStepType GetStepType(string name)
        => string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);
}
