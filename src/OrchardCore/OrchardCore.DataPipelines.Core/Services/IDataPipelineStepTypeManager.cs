using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Gives access to the registered step types.
/// </summary>
public interface IDataPipelineStepTypeManager
{
    /// <summary>
    /// Lists the registered step types, in the order they were registered, without creating them.
    /// </summary>
    /// <returns>The descriptors of the step types.</returns>
    IReadOnlyList<DataPipelineStepTypeDescriptor> GetStepTypes();

    /// <summary>
    /// Finds the descriptor of a step type by its technical name.
    /// </summary>
    /// <param name="name">The technical name of the step type.</param>
    /// <returns>The descriptor, or <see langword="null"/> when no step type has that name.</returns>
    DataPipelineStepTypeDescriptor GetDescriptor(string name);

    /// <summary>
    /// Gets a step type by its technical name, resolved from the services of the current scope.
    /// </summary>
    /// <param name="name">The technical name of the step type.</param>
    /// <returns>The step type, or <see langword="null"/> when no step type has that name.</returns>
    IDataPipelineStepType GetStepType(string name);
}
