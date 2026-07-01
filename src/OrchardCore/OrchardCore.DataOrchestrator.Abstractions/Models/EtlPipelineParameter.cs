namespace OrchardCore.DataOrchestrator.Models;

/// <summary>
/// Represents a parameter definition for an ETL pipeline.
/// </summary>
public sealed class EtlPipelineParameter
{
    /// <summary>
    /// Gets or sets the parameter name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the data type: "String", "Number", "Date", or "Boolean".
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    public string DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets whether this parameter is required.
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets a description of the parameter.
    /// </summary>
    public string Description { get; set; }
}
