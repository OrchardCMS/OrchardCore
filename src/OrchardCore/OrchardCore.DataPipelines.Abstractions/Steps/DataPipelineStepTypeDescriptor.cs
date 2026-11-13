using OrchardCore.Localization;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Describes a registered step type with the static members of its class, so step types are listed, and the
/// categories of steps are known, without creating them. <c>services.AddDataPipelineStepType&lt;TStepType&gt;()</c>
/// registers one per step type.
/// </summary>
public sealed class DataPipelineStepTypeDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataPipelineStepTypeDescriptor"/> class.
    /// </summary>
    /// <param name="name">The stable technical name of the step type.</param>
    /// <param name="displayName">The label shown in the designer.</param>
    /// <param name="description">The description shown in the designer's toolbox.</param>
    /// <param name="category">The category the step is listed under.</param>
    /// <param name="type">The class of the step type, resolved from the services when a step runs.</param>
    public DataPipelineStepTypeDescriptor(
        string name,
        LocalizationSource displayName,
        LocalizationSource description,
        DataPipelineStepCategory category,
        Type type)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(type);

        Name = name;
        DisplayName = displayName;
        Description = description;
        Category = category;
        Type = type;
    }

    /// <summary>
    /// Gets the stable technical name of the step type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the label shown in the designer, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource DisplayName { get; }

    /// <summary>
    /// Gets the description shown in the designer's toolbox, to localize with <c>IStringLocalizerFactory.Localize</c>.
    /// </summary>
    public LocalizationSource Description { get; }

    /// <summary>
    /// Gets the category the step is listed under.
    /// </summary>
    public DataPipelineStepCategory Category { get; }

    /// <summary>
    /// Gets the class of the step type, which implements <see cref="IDataPipelineStepType"/>.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Describes a step type from the static members of its class.
    /// </summary>
    /// <typeparam name="TStepType">The class of the step type.</typeparam>
    /// <returns>The descriptor.</returns>
    public static DataPipelineStepTypeDescriptor Create<TStepType>()
        where TStepType : class, IDataPipelineStepType
    {
        return new DataPipelineStepTypeDescriptor(
            TStepType.Name,
            TStepType.DisplayName,
            TStepType.Description,
            TStepType.Category,
            typeof(TStepType));
    }
}

/// <summary>
/// Holds the registered step types. <c>services.AddDataPipelineStepType&lt;TStepType&gt;()</c> adds them.
/// </summary>
public sealed class DataPipelineStepTypeOptions
{
    private readonly List<DataPipelineStepTypeDescriptor> _stepTypes = [];

    /// <summary>
    /// Gets the registered step types, in the order they were registered.
    /// </summary>
    public IReadOnlyList<DataPipelineStepTypeDescriptor> StepTypes => _stepTypes;

    /// <summary>
    /// Registers a step type, unless its class is already registered.
    /// </summary>
    /// <param name="descriptor">The step type.</param>
    /// <returns>The options.</returns>
    public DataPipelineStepTypeOptions Add(DataPipelineStepTypeDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        if (!_stepTypes.Any(stepType => stepType.Type == descriptor.Type))
        {
            _stepTypes.Add(descriptor);
        }

        return this;
    }
}
