namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Groups the steps in the designer's toolbox, and tells the engine which steps have effects outside the run.
/// </summary>
public enum DataPipelineStepCategory
{
    /// <summary>
    /// A step that reads data, such as a data source or a file.
    /// </summary>
    Source,

    /// <summary>
    /// A step that changes rows, such as a filter, a calculated field or a join.
    /// </summary>
    Transform,

    /// <summary>
    /// A step that turns rows into files, or files into other files.
    /// </summary>
    File,

    /// <summary>
    /// A step that delivers data outside the run, such as saving a file to the media library, sending an email, or
    /// creating content items. Destinations are never executed by a preview.
    /// </summary>
    Destination,
}
