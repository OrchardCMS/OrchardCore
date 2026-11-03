using OrchardCore.DataSources;

namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="SelectFieldsStep"/>.
/// </summary>
public sealed class SelectFieldsStepSettings
{
    /// <summary>
    /// Gets or sets the fields to keep, in order. When empty, every field is kept.
    /// </summary>
    public List<SelectedField> Fields { get; set; } = [];
}

/// <summary>
/// A field kept by a <see cref="SelectFieldsStep"/>.
/// </summary>
public sealed class SelectedField
{
    /// <summary>
    /// Gets or sets the name of the input field.
    /// </summary>
    public string Field { get; set; }

    /// <summary>
    /// Gets or sets the new name of the field, or <see langword="null"/> to keep its name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type to convert the values to, or <see langword="null"/> to keep their type. A value that
    /// can't be converted becomes empty.
    /// </summary>
    public DataFieldType? Type { get; set; }
}
