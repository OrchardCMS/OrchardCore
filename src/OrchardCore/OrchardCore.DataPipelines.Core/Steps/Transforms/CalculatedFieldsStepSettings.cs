namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="CalculatedFieldsStep"/>.
/// </summary>
public sealed class CalculatedFieldsStepSettings
{
    /// <summary>
    /// Gets or sets the fields to calculate, in order.
    /// </summary>
    public List<CalculatedField> Fields { get; set; } = [];
}

/// <summary>
/// A field computed by a formula.
/// </summary>
public sealed class CalculatedField
{
    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the formula, such as <c>[Price] * [Quantity]</c>.
    /// </summary>
    public string Formula { get; set; }
}
