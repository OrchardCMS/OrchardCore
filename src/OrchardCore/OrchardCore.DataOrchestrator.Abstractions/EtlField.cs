namespace OrchardCore.DataOrchestrator;

/// <summary>
/// Represents a dynamic field definition for ETL data mapping.
/// </summary>
public sealed class EtlField
{
    /// <summary>
    /// Gets or sets the field name in the output.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the path to the field in the source data (JSONPath or Liquid expression).
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the data type of the field.
    /// </summary>
    public string Type { get; set; }
}
