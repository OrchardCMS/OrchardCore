namespace OrchardCore.DataPipelines;

/// <summary>
/// The identifiers of the data pipeline features.
/// </summary>
public static class DataPipelinesConstants
{
    /// <summary>
    /// The feature of data pipelines: the designer, the runs, and their steps.
    /// </summary>
    public const string FeatureId = "OrchardCore.DataPipelines";

    /// <summary>
    /// The reusable data layer: data sources, file formats, the formula language and its functions, and data
    /// transforms. It is enabled by the features that depend on it, such as data pipelines or reports.
    /// </summary>
    public const string CoreFeatureId = "OrchardCore.DataPipelines.Core";
}
