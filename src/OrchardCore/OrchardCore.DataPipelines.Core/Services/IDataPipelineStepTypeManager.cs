using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Gives access to the registered step types.
/// </summary>
public interface IDataPipelineStepTypeManager
{
    /// <summary>
    /// Lists the registered step types.
    /// </summary>
    /// <returns>The step types.</returns>
    IReadOnlyList<IDataPipelineStepType> GetStepTypes();

    /// <summary>
    /// Finds a step type by its name.
    /// </summary>
    /// <param name="name">The name of the step type.</param>
    /// <returns>The step type, or <see langword="null"/> when none is registered with that name.</returns>
    IDataPipelineStepType GetStepType(string name);
}
